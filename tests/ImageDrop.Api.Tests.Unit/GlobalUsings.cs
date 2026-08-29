global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Abstractions;
global using Microsoft.AspNetCore.Mvc.Filters;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Routing;

global using System.Security.Claims;

global using ImageDrop.Api.Controllers;
global using ImageDrop.Api.Providers;
global using ImageDrop.Api.Attributes;
global using ImageDrop.Api.Extensions;

global using ImageDrop.Application.Services.Abstractions;
global using ImageDrop.Application.Dtos.Image;
global using ImageDrop.Application.Dtos.Auth;
global using ImageDrop.Application.Dtos.User;

global using NSubstitute;
global using Shouldly;

global using ImageDrop.Shared.Constants.Constants;
global using ImageDrop.Shared.Constants.Core;
global using ImageDrop.Shared.Constants.Enums;