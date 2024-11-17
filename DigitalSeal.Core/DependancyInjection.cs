using DigitalSeal.Core.ListProviders.DocList;
using DigitalSeal.Core.ListProviders.DocPartyList;
using DigitalSeal.Core.ListProviders.DocPartyPossibList;
using DigitalSeal.Core.ListProviders.OrgList;
using DigitalSeal.Core.ListProviders.PartyList;
using DigitalSeal.Core.ListProviders.PartyPendingList;
using DigitalSeal.Core.ListProviders.PartyPossibList;
using DigitalSeal.Core.ListProviders.Signatures;
using DigitalSeal.Core.Services;
using DigitalSeal.Core.Validators;
using DigitalSeal.Data.Models;
using DigitalSeal.Data.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection.Metadata;
using System.Reflection;
using DigitalSeal.Core.Attributes;

namespace DigitalSeal.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddFluentValidationAutoValidation();
            services.AddScoped<IValidator<Organization>, OrganizationValidator>();

            services.AddScoped<IAesService, AesService>();
            services.AddScoped<IUserCertificateProvider, UserCertificateProvider>();
            services.AddScoped<ISignatureService, SignatureService>();
            //services.AddScoped<IDocumentHelper, DocumentHelper>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IDocListProvider, DocListProvider>();
            services.AddScoped<IDocPartyListProvider, DocPartyListProvider>();
            services.AddScoped<IDocPartyPossibListProvider, DocPartyPossibListProvider>();
            services.AddScoped<IOrgListProvider, OrgListProvider>();
            services.AddScoped<IPartyListProvider, PartyListProvider>();
            services.AddScoped<IPartyPendingListProvider, PartyPendingListProvider>();
            services.AddScoped<IPartyPossibListProvider, PartyPossibListProvider>();
            services.AddScoped<ISignaturesListProvider, SignaturesListProvider>();
            services.AddScoped<IDocService, DocService>();
            services.AddScoped<IDocPartyService, DocPartyService>();
            services.AddScoped<IDocPartyValidator, DocPartyValidator>();
            services.AddScoped<IDocPartyNotificationService, DocPartyNotificationService>();
            services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
            services.AddScoped<IOrgService, OrgService>();
            services.AddScoped<IInvitationService, InvitationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISmtpClientWrapper, SmtpClientWrapper>();
            return services;
        }
    }
}
