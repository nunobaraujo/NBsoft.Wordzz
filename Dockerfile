FROM mcr.microsoft.com/dotnet/core/sdk:3.1.200 AS build
WORKDIR /source
COPY ./Wordzz.sln ./

#copy just the project file over
# this prevents additional extraneous restores
# and allows us to re-use the intermediate layer
# This only happens again if we change the csproj.
# This means WAY faster builds!
COPY ./src/Contracts/Wordzz.Contracts.csproj ./src/Contracts/Wordzz.Contracts.csproj
COPY ./src/Wordzz/Wordzz.csproj ./src/Wordzz/Wordzz.csproj

COPY ./tests/Wordzz.Tests/Wordzz.Tests.csproj ./tests/Wordzz.Tests/Wordzz.Tests.csproj

RUN dotnet restore

COPY ./src ./src
RUN dotnet build -c Release --no-restore

RUN dotnet test --logger trx -c Release

RUN dotnet publish "./src/Wordzz/Wordzz.csproj" -c Release -o /app --no-restore 

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY ./src/Wordzz .
EXPOSE 5005
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Wordzz.dll"]
