# Background Remover

**Background Remover** is a Paint.NET plugin that cuts the background out of your images using local ONNX machine-learning models — no internet connection or API keys needed.

There's a discussion thread on the Paint.NET forums here: [Background Remover](https://forums.getpaint.net/topic/133986-background-remover-%E2%80%94-latest-update-2025-07-24/).

## Features

- **AI-based background removal** using ONNX models, run entirely on your machine.
- **GPU acceleration via DirectML**, with automatic fallback to CPU.
- **Optional FP16 model** for lower memory use and faster inference.

## Download

Grab the plugin together with the required ONNX models from the official link:
[https://aka.jjb-pro.com/pdn-bgrem/download](https://aka.jjb-pro.com/pdn-bgrem/download)

> [!WARNING]
> This GitHub repository does not include the model files. The download link above bundles them; see [Building from Source](#building-from-source) if you're building the plugin yourself.

## Getting Started

### Automatic Install

1. Download the latest ZIP from the [official download link](https://aka.jjb-pro.com/pdn-bgrem/download) and extract it.
2. Run **`Open to install.bat`**. It'll prompt for admin rights (needed to write into Paint.NET's Effects folder), then copy everything into place for you.

   If Windows blocks the PowerShell script it calls, you can also just run `installer.ps1` yourself: right-click it → **Run with PowerShell**.

### Manual Install

1. Download and extract the ZIP from the [official download link](https://aka.jjb-pro.com/pdn-bgrem/download).
2. Copy the **BackgroundRemover** folder into your Paint.NET **Effects** folder.
   - Not sure where that is? Right-click your Paint.NET shortcut → **Open file location**, then open the **Effects** folder from there.

## Building from Source

1. Clone the repository:

   ```bash
   git clone https://github.com/jjb-pro/BackgroundRemover.git
   ```

2. Open `BackgroundRemover.slnx` in Visual Studio 2026.
3. Build the solution. By default this also copies the output into `C:\Program Files\paint.net\Effects\BackgroundRemover` — run Visual Studio as Administrator, or change `<PdnRoot>` in `src/BackgroundRemover.csproj`.

You'll still need the ONNX models from the official download link — they're not part of the repo.

## Contributing

Feature requests, bug reports, and pull requests are all welcome.

- Open an [issue](https://github.com/jjb-pro/BackgroundRemover/issues) for bugs or suggestions.
- Fork the repo, make your changes, and create a pull request.
