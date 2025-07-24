# Background Remover

**Background Remover** is an open-source Paint.NET plugin that automatically removes backgrounds from your images using state-of-the-art machine learning models—all running locally with no internet connection required.

## Features

-   **ML-powered Background Removal** – Uses ONNX models for high-quality, accurate background removal

-   **FP16 Precision Support** – Enable half-precision for faster processing on compatible hardware.
    
-   **GPU Acceleration** – Leverage your GPU to speed up background removal if supported.
    
-   **Fully Offline** – Runs entirely on your machine with no need for API keys or internet access. 


## Download

Please **download the plugin along with required ONNX models** from the official link:  
[https://aka.jjb-pro.com/pdn-bgrem/download](https://aka.jjb-pro.com/pdn-bgrem/download)

> ⚠️ The GitHub repository does **not** include the model files due to their size.

## Getting Started

### Automatic Install

1.  Download the latest ZIP from the [official download link](https://aka.jjb-pro.com/pdn-bgrem/download).
    
2.  Inside the ZIP, right-click **install.ps1** and select **Run with PowerShell**.
    
3.  The script will automatically copy all files to the Paint.NET **Effects** folder.
    

### Manual Install

1.  Download and open the main ZIP file from the [official download link](https://aka.jjb-pro.com/pdn-bgrem/download).
    
2.  Locate the inner ZIP file named **BackgroundRemover.zip**.
    
3.  Extract all contents from **BackgroundRemover.zip** into your Paint.NET **Effects** folder.
    
    -   To find the **Effects** folder: right-click your Paint.NET shortcut → **Open file location** → open the **Effects** folder.
        

## Building from Source

1.  Clone the repository:
    
    ```bash
    git clone https://github.com/jjb-pro/BackgroundRemover.git
    ```
    
2.  Open `BackgroundRemover.sln` in Visual Studio 2022.
    
3.  Build the solution and copy the output files to Paint.NET’s **Effects** folder.
    

> Note: You will still need to download the ONNX models separately from the official download link.

## Contributing

Feature requests, bug reports, and pull requests are welcome!

- Open an [issue](https://github.com/jjb-pro/BackgroundRemover/issues) to suggest improvements or report bugs.
    
- Fork the repository, make your changes, and submit a pull request. 

    

## License

MIT License

The Background Remover code is based on [RmbgSharp](https://github.com/ZygoteCode/RmbgSharp) by ZygoteCode. 
