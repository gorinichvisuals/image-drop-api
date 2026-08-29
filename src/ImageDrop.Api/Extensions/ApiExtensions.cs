namespace ImageDrop.Api.Extensions;

public static class ApiExtensions
{    
    extension(IServiceCollection services)
    {
        public void AddOptions(IConfiguration configuration)
        {
            services.Configure<AWSCoreOptions>(configuration.GetSection(nameof(AWSCoreOptions)));
            services.Configure<S3Options>(configuration.GetSection(nameof(S3Options)));
            services.Configure<SqsOptions>(configuration.GetSection(nameof(SqsOptions)));
            services.Configure<RateLimiterOptions>(configuration.GetSection(nameof(RateLimiterOptions)));
        }
        
        public void AddCustomValidationResponse()
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    ICollection<string> validationErrors = context.ModelState
                        .Where(entry => entry.Value!.Errors.Count > 0)
                        .SelectMany(entry => entry.Value!.Errors)
                        .Select(error => error.ErrorMessage)
                        .ToList();

                    string errorMessage = string.Join(
                        Environment.NewLine,
                        validationErrors.Select(message => $"{message}"));

                    ApiResult<object> result = ApiResult<object>.Fail(
                        StatusCodeConstants.BadRequest,
                        ErrorStatusCode.VALIDATION_ERROR,
                        errorMessage);

                    return new BadRequestObjectResult(result);
                };
            });
        }

        public void ConfigureProviders()
        {
            services.AddScoped<ISessionProvider, SessionProvider>();
        }

        public void AddApiCors()
        {
            services.AddCors(
                options =>
                {
                    options.AddDefaultPolicy(
                        builder =>
                        {
                            builder
                                .AllowCredentials()
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .WithOrigins(
                                    "http://localhost:3000");
                        });
                });
        }

        public void AddImageDropSwagger()
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Image Drop API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insert: {token}"
                });
            });
        }

        public void AddApiAuthentication(IConfiguration configuration)
        {
            JwtOptions jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()!;
            SymmetricSecurityKey securityKey = new(Encoding.ASCII.GetBytes(jwtOptions!.JwtSecretKey));

            services.Configure<JwtOptions>(options =>
            {
                options.Issuer = jwtOptions.Issuer;
                options.Audience = jwtOptions.Audience;
                options.AccessTokenExpirationDays = jwtOptions.AccessTokenExpirationDays;
                options.RefreshTokenExpirationDays = jwtOptions.RefreshTokenExpirationDays;
                options.ResetPasswordTokenExpirationDays = jwtOptions.ResetPasswordTokenExpirationDays;
                options.SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            });

            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                RoleClaimType = UserClaimConstants.Role
            };

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(configureOptions =>
                {
                    configureOptions.ClaimsIssuer = jwtOptions.Issuer;
                    configureOptions.TokenValidationParameters = tokenValidationParameters;
                    configureOptions.SaveToken = true;

                    configureOptions.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();

                            context.Response.StatusCode = StatusCodeConstants.Unauthorized;
                            context.Response.ContentType = "application/json";

                            ApiResult result = ApiResult.Fail(
                                StatusCodeConstants.Unauthorized,
                                ErrorStatusCode.UNAUTHORIZED,
                                "Token is missing, invalid, or expired");

                            return context.Response.WriteAsJsonAsync(result);
                        },

                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = StatusCodeConstants.Forbidden;
                            context.Response.ContentType = "application/json";

                            ApiResult result = ApiResult.Fail(
                                StatusCodeConstants.Forbidden, 
                                ErrorStatusCode.FORBIDDEN,
                                "You do not have permission to access this resource");

                            return context.Response.WriteAsJsonAsync(result);
                        }
                    };
                });

            services.AddAuthorization();
        }
        
        public void AddImageDropRateLimiter(IConfiguration configuration, IConnectionMultiplexer connectionMultiplexer)
        {
            RateLimitingOptions rateLimitingOptions = configuration.GetSection(nameof(RateLimitingOptions)).Get<RateLimitingOptions>()!;
            
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodeConstants.TooManyRequests;
                
                options.AddPolicy(
                    RateLimiterPolicyConstants.DefaultPolicy,
                    context =>
                    {
                        string partitionKey = IServiceCollection.GetIpPartitionKey(context);

                        return RedisRateLimitPartition.GetSlidingWindowRateLimiter(
                            partitionKey,
                            _ => new RedisSlidingWindowRateLimiterOptions
                            {
                                ConnectionMultiplexerFactory = () => connectionMultiplexer,
                                PermitLimit = rateLimitingOptions.IpPermitLimit,
                                Window = TimeSpan.FromMinutes(rateLimitingOptions.IpWindowMinutes)
                            });
                    });
                
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodeConstants.TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    ApiResult result = ApiResult.Fail(
                        StatusCodeConstants.TooManyRequests,
                        ErrorStatusCode.TOO_MANY_REQUESTS,
                        "Too many requests. Please try again later.");

                    await context.HttpContext.Response.WriteAsJsonAsync( result, cancellationToken);
                };
            });
        }
        
        private static string GetIpPartitionKey(HttpContext context)
        {
            string ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
            string endpoint = context.Request.Path.Value ?? "unknown-endpoint";

            return $"ip:{endpoint}:{ip}";
        }
    }

    public static void UseImageDropSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = string.Empty;
        });
    }
}