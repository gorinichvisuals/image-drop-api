global using Microsoft.OpenApi;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Filters;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.RateLimiting;

global using Swashbuckle.AspNetCore.Annotations;

global using StackExchange.Redis;
global using RedisRateLimiting;

global using System.Text;
global using System.Security.Claims;

global using ImageDrop.AWS.SecretsManager.Services;

global using ImageDrop.AWS.S3.Extensions;

global using ImageDrop.AWS.SQS.Publisher.Extensions;

global using ImageDrop.Api.Providers;
global using ImageDrop.Api.Extensions;
global using ImageDrop.Api.Attributes;

global using ImageDrop.Application.Services.Abstractions;
global using ImageDrop.Application.Extensions;
global using ImageDrop.Application.Dtos.Auth;
global using ImageDrop.Application.Dtos.User;
global using ImageDrop.Application.Dtos.Image;

global using ImageDrop.Persistence.Extensions;

global using ImageDrop.Shared.Constants.Core;
global using ImageDrop.Shared.Constants.Enums;
global using ImageDrop.Shared.Constants.Constants;
global using ImageDrop.Shared.Constants.Options;