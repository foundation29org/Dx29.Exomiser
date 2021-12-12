#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Dx29.Exomiser.WebAPI/Dx29.Exomiser.WebAPI.csproj", "Dx29.Exomiser.WebAPI/"]
COPY ["Dx29.Azure/Dx29.Azure.csproj", "Dx29.Azure/"]
COPY ["Dx29/Dx29.csproj", "Dx29/"]
COPY ["Dx29.Exomiser/Dx29.Exomiser.csproj", "Dx29.Exomiser/"]
COPY ["Dx29.Jobs/Dx29.Jobs.csproj", "Dx29.Jobs/"]
RUN dotnet restore "Dx29.Exomiser.WebAPI/Dx29.Exomiser.WebAPI.csproj"
COPY . .
WORKDIR "/src/Dx29.Exomiser.WebAPI"
RUN dotnet build "Dx29.Exomiser.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dx29.Exomiser.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dx29.Exomiser.WebAPI.dll"]