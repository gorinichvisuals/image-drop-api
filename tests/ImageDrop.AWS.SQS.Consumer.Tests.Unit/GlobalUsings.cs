global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.DependencyInjection;

global using System.Text.Json;

global using ImageDrop.AWS.SQS.Consumer.Handlers;
global using ImageDrop.AWS.SQS.Consumer.Helper;
global using ImageDrop.AWS.SQS.Consumer.Dispatcher;

global using ImageDrop.Shared.Services.Services.Abstractions;

global using ImageDrop.Shared.SQS.Contracts.Messages;
global using ImageDrop.Shared.SQS.Contracts.Abstractions;

global using Shouldly;
global using NSubstitute;
global using NSubstitute.ExceptionExtensions;

global using Amazon.SQS;
global using Amazon.SQS.Model;