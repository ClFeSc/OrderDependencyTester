# OrderDependencyTester

This project provides a C# implementation of an algorithm determining whether [Order Dependencies (ODs)](https://hpi.de/fileadmin/user_upload/fachgebiete/naumann/publications/PDFs/2022_schmidl_efficient.pdf) hold, given a Ground Truth set of known-good ODs.

## Setup

### Technical requirements

The project uses C# version 12 with [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

## Usage

**Note**: This project is in its very early stages. It is possible, and even likely, that the interface of the program will change in the future, potentially without updating this section.

The frontend lives in the [`CliFrontend`](./CliFrontend) project.
It requires three (3) arguements, all of which are file paths:

* A file containing the known set-based ODs (Format: `{Column1, Column2}: Column3↓ ~ Column4↑` and `{Column1, Column2}: [] ↦ Column3` respectively).
* A file containing the list-based ODs to test (Format: `[Column1↓, Column2↑] -> [Column3↑,Column4↓]`.
* A file containing all attributes of the relation of the ODs (Format: one line per attribute).

All of these files contain one line per item

The program will output for each given list-based OD whether it holds under the condition that the set-based ODs hold.

### Run

From the project root, execute

```bash
dotnet run --project CliFrontend <path to set-based ground truth file> <path to list-based candidate file> <path to all attributes file>
```

## Algorithm

**Note**: This project is in its very early stages. It is possible, and even likely, that the algorithm will change in the future, potentially without updating this section.

**Note**: This section needs expanding.

The algorithm uses the well-known FD membership algorithm by [Bernstein and Beeri](https://ia800105.us.archive.org/24/items/technicalreportc73univ/technicalreportc73univ.pdf) to validate constant ODs.
For order-compatible ODs, first the Propagate rule is applied.
The remaining ODs will then be validated using the remaining axioms.
