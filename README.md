# V-C-Wrapper-Generator
Helper for generating V wrapper code from C source


## Requirements for Building
- dotnet Core 3


## Building
- open the project in VS Code and use the pre-configured build task (Ctrl/Cmd + Shift + b)


## Usage
- **help**: run `./VGenerator -h` to see help on the command line
- **config**: run `./VGenerator -c filename.json` to create a template file which will dictate how V generation is done. There are several example configs in the `Program.cs` file as well.
- **generate**: run `./VGenerator -g filename.json` to generate the V wrapper
