using DemoApp.ViewModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.DemoApp.Xamarin.Models;
using Xamarin.Plugins.FluentValidation;

namespace WebRTCme.DemoApp.Xamarin.Validators
{
    public class CallParametersValidator : EnhancedAbstractValidator<ConnectionParametersViewModel>
    {
        public CallParametersValidator()
        {
            RuleForProp(prop => prop.RoomName)
                .NotNull()
                .NotEmpty();

            RuleForProp(prop => prop.UserName)
                .NotNull()
                .NotEmpty();

        }
    }
}
