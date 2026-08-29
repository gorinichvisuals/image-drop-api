global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.Extensions.Caching.Distributed;

global using System.Text;
global using System.Text.Json;
global using System.Linq.Expressions;

global using ImageDrop.Application.Dtos.Auth;
global using ImageDrop.Application.Dtos.User;
global using ImageDrop.Application.Dtos.Image;
global using ImageDrop.Application.Services.Abstractions;
global using ImageDrop.Application.Services.Implementations;

global using ImageDrop.Persistence.Context.Entities;
global using ImageDrop.Persistence.Repositories.UoW;

global using ImageDrop.Shared.Constants.Constants;
global using ImageDrop.Shared.Constants.Core;
global using ImageDrop.Shared.Constants.Enums;
global using ImageDrop.Shared.Constants.Options;

global using ImageDrop.Shared.SQS.Contracts.Messages;

global using Shouldly;
global using NSubstitute;
global using NSubstitute.ExceptionExtensions;

global using ImageDrop.AWS.S3.Services;
global using ImageDrop.AWS.SQS.Publisher.Services;