global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Configuration;

global using System.Net;
global using System.Text.Json;
global using System.Reflection;

global using Amazon.SQS;
global using Amazon.SQS.Model;
global using Amazon.Runtime;

global using ImageDrop.AWS.SQS.Consumer.Helper;
global using ImageDrop.AWS.SQS.Consumer.Handlers;
global using ImageDrop.AWS.SQS.Consumer.Dispatcher;
global using ImageDrop.AWS.SQS.Consumer.Services.Abstractions;
global using ImageDrop.AWS.SQS.Consumer.Services.Implementations;

global using ImageDrop.Shared.SQS.Contracts.Abstractions;
global using ImageDrop.Shared.SQS.Contracts.Messages;

global using ImageDrop.Shared.Services.Extensions;
global using ImageDrop.Shared.Services.Services.Abstractions;

global using ImageDrop.Shared.Constants.Options;

global using ImageDrop.Persistence.Extensions;

global using ImageDrop.AWS.S3.Extensions;