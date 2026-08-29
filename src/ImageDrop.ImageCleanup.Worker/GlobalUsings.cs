global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;

global using Quartz;

global using ImageDrop.ImageCleanup.Worker.Jobs;

global using ImageDrop.Shared.Services.Extensions;
global using ImageDrop.Shared.Services.Services.Abstractions;

global using ImageDrop.Shared.Constants.Options;

global using ImageDrop.Persistence.Extensions;

global using ImageDrop.AWS.S3.Extensions;

global using ImageDrop.AWS.SecretsManager.Services;