# Gaze Interaction Apps

## Overview

The Gaze Interaction Apps repository is a collection of samples which demonstrate the use of the [Gaze Interaction library](https://github.com/Microsoft/WindowsCommunityToolkit/tree/master/Microsoft.Toolkit.Uwp.Input.GazeInteraction), part of the [Windows Community Toolkit](https://github.com/Microsoft/WindowsCommunityToolkit/). Please refer to the [documentation](https://github.com/Microsoft/WindowsCommunityToolkit/blob/master/docs/gaze/GazeInteractionLibrary.md) for general usage. The Gaze Interaction library is available on [nuget](https://www.nuget.org/packages/Microsoft.Toolkit.Uwp.Input.GazeInteraction/), such that searching for `GazeInteraction` should find the currently released build.

## Build notes

This project is often using unreleased copies of the GazeInteraction library as a means of testing new features. As such, you may run into an error such as this:

```Error
Unable to find package Microsoft.Toolkit.Uwp.Input.GazeInteraction with version (>= 3.1.0-build.50)
```

To resolve this, you will need to point your Visual Studio nuget package manager to this feed: [https://dotnet.myget.org/F/uwpcommunitytoolkit/api/v3/index.json](https://dotnet.myget.org/F/uwpcommunitytoolkit/api/v3/index.json).
You may do so by going into `Tools->Nuget Package Manager->Package Manager Settings`. Select `Package Sources`. Click the green `+` to create a new package source and specify [https://dotnet.myget.org/F/uwpcommunitytoolkit/api/v3/index.json](https://dotnet.myget.org/F/uwpcommunitytoolkit/api/v3/index.json) as the source. The name is up to you, but `Windows Community Toolkit Preview Builds` is a reasonable choice.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
