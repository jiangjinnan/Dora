dotnet clean
dotnet restore
dotnet build -c Release
dotnet pack ./Interception  -c Release -o ./packages --include-symbols
dotnet pack ./Interception.AspNetCore  -c Release -o ./packages --include-symbols
pause