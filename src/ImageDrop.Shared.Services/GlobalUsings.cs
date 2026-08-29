global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;

global using ImageDrop.Shared.Services.Dtos.Image;
global using ImageDrop.Shared.Services.Services.Abstractions;
global using ImageDrop.Shared.Services.Services.Implementations;

global using ImageDrop.Shared.Constants.Enums;
global using ImageDrop.Shared.Constants.Options;
global using ImageDrop.Shared.Constants.Core;

global using SixLabors.ImageSharp;
global using SixLabors.ImageSharp.Formats.Jpeg;
global using SixLabors.ImageSharp.Formats.Png;

global using ImageDrop.AWS.S3.Services;

global using ImageDrop.Persistence.Repositories.UoW;
