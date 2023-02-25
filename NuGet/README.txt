ISSUE
=====
When you create a NuGet package from a project by ticking 'Generate NuGet package on build' option from 'Properties->Package' selection,
it will execute 'dotnet pack {current}.csproj' command. This will bundle the DLL (and static assets from wwwroot folder for RCL projects)
of the current project, NOT the DLLs from referenced projects. 
When you examine the generated '.nuspec' file on 'obj' folder, you will see that all referenced projects are marked as dependency and no DLLs
or static assests from these projects will be packaged. This is by design and each dependency needs to be dotnet packed separately!!!.
See this article about it:
https://josef.codes/dotnet-pack-include-referenced-projects/

WORKAROUNDS
===========
1.) Use the following command to include DLLs from dependent projects:
'nuget pack {current}.csproj -IncludeReferencedProjects'
2.) Use the tricks provided by the above article. I used the first option in first release of the 'WebRTCme' framework. For example 'WebRTCme.proj'
is using:
  <PropertyGroup>
    <TargetsForTfmSpecificBuildOutput>$(TargetsForTfmSpecificBuildOutput);CopyProjectReferencesToPackage</TargetsForTfmSpecificBuildOutput>
  </PropertyGroup>
<Target Name="CopyProjectReferencesToPackage" DependsOnTargets="ResolveReferences">
    <ItemGroup>
      <BuildOutputInPackage Include="@(ReferenceCopyLocalPaths->WithMetadataValue('ReferenceSourceTarget', 'ProjectReference'))" />
    </ItemGroup>
  </Target>
and add 'PrivateAssets="All"' to all 'ProjectReference' tags.

None of the above solutions provides the static assests from wwwroot folder of RCL projects!!! Only DLLs!!!

THE REAL SOLUTION
=================
As 'WebRTCme framework' uses a RCL project (WebRTCme.Bindinds.Blazor) as a base, the above workarounds will not work. Currently the only solution
is:
- Get the generated '.nuspec' files from all projects
- Merge them manually according to the needs
- Use 'nuget pack {current}.nuspec' command to generate the desired NuGet.
For this purpose the 'NuGet' folder is created and NuGet generations will be done there.

BUILD PACKAGE
=============
nuget pack <name>.nuspec


LINKS
=====
https://stackoverflow.com/questions/15882770/creating-one-nuget-package-from-multiple-projects-in-one-solution
https://josef.codes/dotnet-pack-include-referenced-projects/





