# Uri Templates #

##2.0.0
- [Breaking] Updated Target Framework Moniker to `netstandard2.0` and drops `net35`, `net40`, `net45` and `netstandard1.0`.

##1.1.2
- Added Type converter support 

##1.1.1
- Fixed bug parsing query parameter with comma delimited values


##1.1.0
- Updated Target Framework Moniker from dotnet to netstandard1.0

##1.0.0
- This project has lived too long as a 0.x release.  I believe it has seen enough production use to be considered a 1.0 release.

##0.6.6
- Bugfix

##0.6.5
- Added ability to retrieve URITemplates from UriTemplateTable
- Parameter matching

##0.6.4

- Added .net4 version of assembly
- Updated nuget to put portable lib in dotnet folder to enable coreclr support
- Made Resolve() thread safe by ensuring it does not share any state from one invocation to the next.
- Added support for profile92 to allow including in Portable libraries that target .net4
- Added support for case insensitive parameter names.

##0.6.3

- Added ToString() overload to allow retrieving unresolved template

##0.6.2

- URI Template Extension AddParameters now uses IDictionary instead of Dictionary

##0.6.1

- Added ClearParameter to unset a template parameter
- Added MakeTemplate URI extension for creating a Uri template based on the query string parameters of a URI
- Added GetQueryStringParameters URI extension for building dictionary of query parameters and values
- Added AddParameters overload that accepts a dictionary of template parameters

##0.6.0

- Added the ability to partially resolve templates using a new constructor parameter.
- Added new fluent interface using extension methods for quickly creating a template and resolving it.
- Created a .net 45 project
- Restructured folders to comply with recommendations made by David Fowler
- Added many more usage tests with real world scenarios
- Added support for Windows 8.1
- Fixed Unicode encoding problem
- Added support for passing non-string lists as parameters
- Added support for passing parameters values that are non-string.
