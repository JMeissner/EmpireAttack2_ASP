#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["EmpireAttack2_ASP/EmpireAttack2_ASP.csproj", "EmpireAttack2_ASP/"]
RUN dotnet restore "EmpireAttack2_ASP/EmpireAttack2_ASP.csproj"
COPY . ./
WORKDIR "/src/EmpireAttack2_ASP"
RUN dotnet build "EmpireAttack2_ASP.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "EmpireAttack2_ASP.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "EmpireAttack2_ASP.dll"]