using System;
using System.Linq;
using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;

namespace RjbSolutions.CalculateRollupField
{
	/// <summary>
	/// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
	/// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
	/// </summary>
	public class CalculateRollupFieldAsActionPlugin : PluginBase
	{
		/// <summary>
		/// Custom Action cannot have a unsecureConfiguration or  secureConfiguration
		/// </summary>
		public CalculateRollupFieldAsActionPlugin(/* string unsecureConfiguration, string secureConfiguration */)
			: base(typeof(CalculateRollupFieldAsActionPlugin))
		{
		}

		// Entry point for custom business logic execution
		protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
		{
			if (localPluginContext == null)
				throw new ArgumentNullException(nameof(localPluginContext));

			var context = localPluginContext.PluginExecutionContext;

			if (!context.MessageName.StartsWith("pw_CalculateRollupField", StringComparison.InvariantCulture) || context.Stage != 30)
			{
				localPluginContext.Trace($"pw_CalculateRollupField plug-in is not associated with the expected message or is not registered for the main operation. Message: {context.MessageName}; Stage: {context.Stage}");
				return;
			}

			EntityReference entityReference = context.InputParameterOrDefault<EntityReference>("Target")
					?? throw new FaultException<OrganizationServiceFault>(new OrganizationServiceFault(), "Target is not given");

			string fieldName = context.InputParameterOrDefault<string>("FieldName")
					?? context.MessageName.Split('.').LastOrDefault()?.ToLowerInvariant()
					?? throw new FaultException<OrganizationServiceFault>(new OrganizationServiceFault(), "FieldName is not given");

			CalculateRollupFieldRequest request = new CalculateRollupFieldRequest()
			{
				Target = entityReference,
				FieldName = fieldName,
			};

			var response = localPluginContext.PluginUserService.Execute(request) as CalculateRollupFieldResponse;
			context.OutputParameters["Result"] = response.Entity;
		}
	}
}
