FROM <uri>/exomiser:v12.1.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0.102-ca-patch-buster-slim AS build

WORKDIR /src
COPY ["Dx29.Exomiser.Worker/Dx29.Exomiser.Worker.csproj", "Dx29.Exomiser.Worker/"]
COPY ["Dx29.Exomiser/Dx29.Exomiser.csproj", "Dx29.Exomiser/"]
COPY ["Dx29.Jobs/Dx29.Jobs.csproj", "Dx29.Jobs/"]
COPY ["Dx29.Azure/Dx29.Azure.csproj", "Dx29.Azure/"]
COPY ["Dx29/Dx29.csproj", "Dx29/"]
RUN dotnet restore "Dx29.Exomiser.Worker/Dx29.Exomiser.Worker.csproj"
COPY . .
WORKDIR "/src/Dx29.Exomiser.Worker"
RUN dotnet build "Dx29.Exomiser.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dx29.Exomiser.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dx29.Exomiser.Worker.dll"]
