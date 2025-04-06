# This is a multi-stage build. In Dockerfile the "as" keyword is used to name a stage in a multi-stage build. 
# This allow us to refer to the stage later in the Dockerfile.

# In a multi-stage Dockerfile, you create different stages (or layers) to separate the build process into distinct steps. 
# Each stage can use a different base image that provides the necessary tools for that specific step. 
# This approach helps to keep the final image smaller and more secure by including only the necessary runtime components.

# In a multi-stage Dockerfile, each stage starts with a new environment based on the specified base image. 
# This means that each stage has its own file system and environment, independent of the other stages, 
# unless files are explicitly copied from one stage to another. That's why "WORKDIR /app" is specified several times."

# Here "as base" names the first stage "base". This stage uses the mcr.microsoft.com/dotnet/aspnet:9.0 image and sets the working directory to /app.
# Later for the last stage "base" stage is referenced again. This means that the final stage will start from the base stage,
# inheriting its layers and configurations.
# Multi-stage builds help reduce the final image size by allowing you to copy only the necessary artifacts from previous stages, avoiding the
# inclusion of build tools and intermediate files in the final image.

# The reason for pulling mcr.microsoft.com/dotnet/aspnet:9.0 in the base stage, even though it is only used in the final stage, 
# is to define a reusable base image that can be referenced later. This approach helps to keep the Dockerfile organized 
# and makes it clear which image is being used as the runtime environment for the final stage.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# dotnet restore command requires the .NET SDK, which is why we are using the mcr.microsoft.com/dotnet/sdk:9.0 image in this layer. The SDK image
# includes the necessary tools and dependencies to restore, build, and publish .NET applications, whereas the ASP>NET runtime image
# (mcr.microsoft.com/dotnet/aspnet:9.0) only includes the runtime components needed to run the application.
# Here's a brief overview of the images used:
# •	mcr.microsoft.com/dotnet/aspnet:9.0: Contains only the ASP.NET runtime, used for running the application.
# •	mcr.microsoft.com/dotnet/sdk:9.0: !!! is lightweight !!! Contains the .NET SDK, which includes the tools needed for building and publishing .NET applications, including dotnet restore.
# By using the SDK image in the build stage, you ensure that all necessary tools are available to restore the project's dependencies.

# The reason for copying the .csproj file and the nuget.config and restoring dependencies first, before copying the rest of the application files,
# is to take advantage of Docker's layer caching mechanism. This approach can significantly speed up the build process, espescially when you
# make frequent changes to your source code.
# - By copying only the ".csproj" file and "nuget.config" first, you ensure that the "dotnet restore" command runs only when there are changes to
# the project file or dependencies.
# - Docker caches each layer, so if the ".csproj" file and "nuget.config" haven't changed, Docker will use the cached layer and skip the 
# "dotnet restore" step, which can be time-consuming.

# By structuring the Dockerfile this way, you optimize the build process by minimizing the number of times the dotnet restore command needs to run,
# leveraging Docker's caching mechanism to speed up builds when only the source code changes.

# If, for example you make changes in Auth.Service.csproj file
# - The "COPY ["auth-microservice/Auth.Service/Auth.Service.csproj", "Auth.Service/"]" command will detect the change and invalidate the cache
# for this layer
# - The RUN dotnet restore "Auth.Service/Auth.Service.csproj" command will be executed again to restore the dependencies based on the updated
# .csproj file.
# Subsequent layers that depend on the restored dependencies will also be rebuilt, but the dotnet restore step will only be re-executed 
# if there are changes to the .csproj file or nuget.config.
# This ensures that any changes to the project's dependencies are properly restored and included in the build process.

# "ARG BUILD_CONFIGURATION=Release" defines a build-time variable named BUILD_CONFIGURATION with a default value of Release. 
# This variable can be used in subsequent commands within the Dockerfile to specify the build configuration for your .NET application.
# allows you to easily switch between different build configurations (e.g., Debug or Release) without modifying the Dockerfile. 
# You can override the default value by passing a different value when building the Docker image:
# docker build --build-arg BUILD_CONFIGURATION=Debug -t myapp:debug . (This is docker command).

# Absolute Path: "COPY ["local-nuget-packages/", "/local-nuget-packages/"]"
# File System Structure:
# /
# ├── Auth.Service
# │   └── (other files and directories)
# ├── local-nuget-packages
# │   ├── (contents of local-nuget-packages directory)
# │   └── ...
# └── (other files and directories)

# Relative Path: "COPY ["local-nuget-packages/", "local-nuget-packages/"]"
# /
# ├── Auth.Service
# │   ├── local-nuget-packages
# │   │   ├── (contents of local-nuget-packages directory)
# │   │   └── ...
# │   └── (other files and directories)
# └── (other files and directories)

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
# "/" at the end of dest file means Docker will copy the `Auth.Service.csproj` file into the `Auth.Service` directory. If the `Auth.Service` directory does not exist, Docker will create it.
COPY ["auth-microservice/Auth.Service/Auth.Service.csproj", "Auth.Service/"]
# "/" in front means absolute path definition for tis is up.
COPY ["local-nuget-packages/", "/local-nuget-packages/"] 
COPY ["auth-microservice/nuget.config", "./"]
RUN dotnet restore "Auth.Service/Auth.Service.csproj"

# - After restoring the dependencies, we copy the rest of the application files into the container.
# - Since the source code files are copied in a separate layer, changes to the source code files will not invalidate the cached 
# "dtonet restore" layer, making subsequent builds faster.
# This means that changes in files other than nuget.config or Auth.Service.csproj, will not trigger the dotnet restore command to run again.
# For example changes in file "appsettings.json" won't run dotnet restore.

COPY ["auth-microservice/Auth.Service/", "Auth.Service/"]
# Sets the working directory to Auth.Service, which means that subsequent commands will be executed relative to this directory. This is necessary because the Auth.Service.csproj file is located in the Auth.Service directory.
WORKDIR "Auth.Service" 
RUN dotnet build "Auth.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage continues from the "build" stage and uses the "dotnet publish" command to compile the application and prepare it for deployment.
# It continues from the "build" stage because it uses publish command which is in .Net SDK and we already have this image is "build" stage.

FROM build as publish
RUN dotnet publish "Auth.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish

# This stage uses the "base" stage (ASP.NET runtime image) and copies the published output from the "publish" stage. It sets the entrypoint to run the application.
# - The `ENTRYPOINT` command specifies the command to run the application.

# The COPY --from=publish /app/publish . command is used to copy files from a previous build stage to the final stage. Here's a detailed explanation:
# 1.  --from=publish:
#   •	This specifies the source stage from which to copy files. In your Dockerfile, publish is the name of the stage where the application is published.
#   •	The publish stage is defined earlier in the Dockerfile with FROM build as publish.
# 2.  /app/publish:
#   •	This is the source directory in the publish stage from which files will be copied.
#   •	In the publish stage, the dotnet publish command outputs the published application to the /app/publish directory.
# 3.  . (dot):
#   •	This is the destination directory in the current stage (the final stage) where the files will be copied.
#   •	The dot (.) represents the current working directory, which is set to /app in the final stage.

# Putting the last one all together, the "COPY --from=publish /app/publish ." means:
# •	Copy the contents of the /app/publish directory from the publish stage.
# •	Place the copied contents into the current working directory (/app) of the final stage.
# This ensures that the final Docker image contains only the necessary files to run the application, 
# without including any of the build tools or intermediate files from the previous stages.

FROM base as final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.Service.dll"]