global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Caching.Distributed;

global using System.Text.Json;
global using System.Security.Claims;
global using System.IdentityModel.Tokens.Jwt;
global using System.ComponentModel.DataAnnotations;

global using StackExchange.Redis;

global using Isopoh.Cryptography.Argon2;

global using ImageDrop.Application.Services.Abstractions;
global using ImageDrop.Application.Services.Implementations;
global using ImageDrop.Application.Dtos.Common;
global using ImageDrop.Application.Dtos.Auth;
global using ImageDrop.Application.Dtos.User;
global using ImageDrop.Application.Dtos.Image;

global using ImageDrop.Persistence.Repositories.UoW;
global using ImageDrop.Persistence.Context.Entities;

global using ImageDrop.Shared.Constants.Enums;
global using ImageDrop.Shared.Constants.Core;
global using ImageDrop.Shared.Constants.Options;
global using ImageDrop.Shared.Constants.Constants;

global using ImageDrop.AWS.S3.Services;

global using ImageDrop.AWS.SQS.Publisher.Services;

global using ImageDrop.Shared.SQS.Contracts.Messages;