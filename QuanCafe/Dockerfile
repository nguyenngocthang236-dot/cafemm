FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["QuanCafe/QuanCafe.csproj", "QuanCafe/"]
RUN dotnet restore "QuanCafe/QuanCafe.csproj"
COPY . .
WORKDIR "/src/QuanCafe"
RUN dotnet build "QuanCafe.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QuanCafe.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuanCafe.dll"]