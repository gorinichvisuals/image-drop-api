global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Logging;

global using System.Linq.Expressions;

global using NSubstitute;
global using Shouldly;

global using SixLabors.ImageSharp;
global using SixLabors.ImageSharp.Formats.Jpeg;
global using SixLabors.ImageSharp.PixelFormats;

global using ImageDrop.Shared.Constants.Enums;
global using ImageDrop.Shared.Constants.Options;
global using ImageDrop.Shared.Constants.Constants;
global using ImageDrop.Shared.Constants.Core;

global using ImageDrop.Shared.Services.Services.Implementations;
global using ImageDrop.Shared.Services.Services.Abstractions;
global using ImageDrop.Shared.Services.Dtos.Image;

global using ImageDrop.AWS.S3.Services;

global using ImageDrop.Persistence.Repositories.UoW;