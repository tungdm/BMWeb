﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace TGVL.Models
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class DateRange : ValidationAttribute, IClientValidatable
    {
        private const string DefaultErrorMessage = "Receiving date must be on or after today";

        public DateRange() : base(DefaultErrorMessage) {  }

        public override string FormatErrorMessage(string name)
        {
            if (ErrorMessage == null)
            {
                return string.Format(DefaultErrorMessage, name);
            }
            return string.Format(ErrorMessage, name);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dateEntered = (DateTime)value;
            if (dateEntered < DateTime.Today)
            {
                var message = FormatErrorMessage(dateEntered.ToShortDateString());
                return new ValidationResult(message);
            }
            return null;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientCustomDateValidationRule(FormatErrorMessage(metadata.DisplayName));
            yield return rule;
        }
    }

    internal class ModelClientCustomDateValidationRule : ModelClientValidationRule
    {
        public ModelClientCustomDateValidationRule(string errorMessage)
        {
            ErrorMessage = errorMessage;
            ValidationType = "daterange";
        }
    }
}