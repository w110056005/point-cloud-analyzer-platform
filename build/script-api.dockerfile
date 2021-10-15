#FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
COPY ./src /src
WORKDIR /src

#release to target folder
RUN dotnet publish script-executor-web-api -o /publish --configuration Release

#FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app

COPY --from=build /publish .

# Install Open3D system dependencies and pip
RUN apt-get update && apt-get install --no-install-recommends -y \
    gcc \
    libgl1 \
    libgomp1 \
    libusb-1.0-0 \
    python3-pip \
    && rm -rf /var/lib/apt/lists/*
RUN python3 -m pip install --upgrade pip
RUN python3 -m pip install --upgrade setuptools

# Install Open3D from the pypi repositories
RUN python3 -m pip install --no-cache-dir open3d

ENTRYPOINT ["dotnet", "script-executor-web-api.dll", "--environment=Development"]