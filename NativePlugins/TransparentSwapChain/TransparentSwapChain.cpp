// TransparentSwapChain — Unity native plugin for D3D12 transparent window rendering.
//
// Hooks IDXGIFactory2::CreateSwapChainForHwnd via COM vtable patching.
// Redirects to CreateSwapChainForComposition with DXGI_ALPHA_MODE_PREMULTIPLIED,
// then binds the swap chain to a DirectComposition visual.
// Unity renders directly into the alpha-aware composition swap chain — no frame copy.
//
// Requirements:
//   - Windows 10 1903+ or Windows 11
//   - preserveFramebufferAlpha = true in PlayerSettings
//   - Camera background alpha = 0
//   - DwmExtendFrameIntoClientArea called with negative margins
//   - DLL must be set to "Preloaded" in Unity Inspector
//
// Build (x64 Native Tools Command Prompt):
//   cl /LD /O2 /EHsc TransparentSwapChain.cpp /link dxgi.lib dcomp.lib

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <dxgi1_3.h>
#include <dcomp.h>

#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "dcomp.lib")

// --- Hook types ---

typedef HRESULT(STDMETHODCALLTYPE* PFN_CreateSwapChainForHwnd)(
    IDXGIFactory2* pThis,
    IUnknown* pDevice,
    HWND hWnd,
    const DXGI_SWAP_CHAIN_DESC1* pDesc,
    const DXGI_SWAP_CHAIN_FULLSCREEN_DESC* pFullscreenDesc,
    IDXGIOutput* pRestrictToOutput,
    IDXGISwapChain1** ppSwapChain);

// --- State ---

static PFN_CreateSwapChainForHwnd s_Original = nullptr;
static IDCompositionDevice* s_DCompDevice = nullptr;
static IDCompositionTarget* s_DCompTarget = nullptr;
static IDCompositionVisual* s_DCompVisual = nullptr;
static bool s_HookInstalled = false;

// --- Swap chain creation hook ---

static HRESULT STDMETHODCALLTYPE Hook_CreateSwapChainForHwnd(
    IDXGIFactory2* pFactory,
    IUnknown* pDevice,
    HWND hWnd,
    const DXGI_SWAP_CHAIN_DESC1* pDesc,
    const DXGI_SWAP_CHAIN_FULLSCREEN_DESC* pFullscreen,
    IDXGIOutput* pOutput,
    IDXGISwapChain1** ppSwapChain)
{
    DXGI_SWAP_CHAIN_DESC1 desc = *pDesc;

    // Enables alpha.
    desc.AlphaMode = DXGI_ALPHA_MODE_PREMULTIPLIED; // DXGI_ALPHA_MODE_UNSPECIFIED, default
    desc.Scaling = DXGI_SCALING_STRETCH;

    if (desc.Width == 0 || desc.Height == 0)
    {
        RECT rc;
        GetClientRect(hWnd, &rc);
        desc.Width = rc.right - rc.left;
        desc.Height = rc.bottom - rc.top;
    }

    HRESULT hr = pFactory->CreateSwapChainForComposition(
        pDevice, &desc, pOutput, ppSwapChain);

    if (FAILED(hr))
        return s_Original(pFactory, pDevice, hWnd, pDesc, pFullscreen, pOutput, ppSwapChain);

    // Tear down previous DirectComposition state
    if (s_DCompVisual) { s_DCompVisual->Release(); s_DCompVisual = nullptr; }
    if (s_DCompTarget) { s_DCompTarget->Release(); s_DCompTarget = nullptr; }
    if (s_DCompDevice) { s_DCompDevice->Release(); s_DCompDevice = nullptr; }

    hr = DCompositionCreateDevice(
        nullptr, __uuidof(IDCompositionDevice),
        reinterpret_cast<void**>(&s_DCompDevice));

    if (FAILED(hr))
    {
        (*ppSwapChain)->Release();
        *ppSwapChain = nullptr;
        return s_Original(pFactory, pDevice, hWnd, pDesc, pFullscreen, pOutput, ppSwapChain);
    }

    s_DCompDevice->CreateTargetForHwnd(hWnd, TRUE, &s_DCompTarget);
    s_DCompDevice->CreateVisual(&s_DCompVisual);
    s_DCompVisual->SetContent(*ppSwapChain);
    s_DCompTarget->SetRoot(s_DCompVisual);
    s_DCompDevice->Commit();

    return S_OK;
}

// --- Hook installation ---

static void InstallHook()
{
    if (s_HookInstalled)
        return;

    // Temporal factory instance to find out vtable address.
    IDXGIFactory2* factory = nullptr;
    if (FAILED(CreateDXGIFactory2(0, __uuidof(IDXGIFactory2),
            reinterpret_cast<void**>(&factory))))
        return;

    // ptr_vtable is hidden field at 0 index.
    // static virtual function can be found throught vtable: variable->&object->&vtable->&function
    void** vt = *reinterpret_cast<void***>(factory);

    // CreateSwapChainForHwnd is at vtable index 15
    // IUnknown(3) + IDXGIObject(4) + IDXGIFactory(5) + IDXGIFactory1(2) + IDXGIFactory2[1] = 15
    s_Original = reinterpret_cast<PFN_CreateSwapChainForHwnd>(vt[15]);

    // Unlock protection -> overwrite function -> restore protection.
    DWORD oldProtect;
    if (VirtualProtect(&vt[15], sizeof(void*), PAGE_READWRITE, &oldProtect))
    {
        vt[15] = reinterpret_cast<void*>(&Hook_CreateSwapChainForHwnd);
        VirtualProtect(&vt[15], sizeof(void*), oldProtect, &oldProtect);
        s_HookInstalled = true;
    }

    // Dispose temp factory.
    factory->Release();
}

// --- Unity plugin entry points ---

struct IUnityInterfaces;

extern "C"
{
    __declspec(dllexport) void UnityPluginLoad(IUnityInterfaces*)
    {
        InstallHook();
    }

    __declspec(dllexport) void UnityPluginUnload()
    {
        if (s_DCompVisual) { s_DCompVisual->Release(); s_DCompVisual = nullptr; }
        if (s_DCompTarget) { s_DCompTarget->Release(); s_DCompTarget = nullptr; }
        if (s_DCompDevice) { s_DCompDevice->Release(); s_DCompDevice = nullptr; }
    }
}
