using System;
using System.ComponentModel;
using System.Web.Mvc;

namespace Common.ModelBinders
{
    public class BirthDateModelBinder : DefaultModelBinder
    {
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor.PropertyType == typeof(DateTime?) || propertyDescriptor.PropertyType == typeof(DateTime))
            {
                var request      = controllerContext.HttpContext.Request;
                var propertyName = propertyDescriptor.Name;

                var date = string.Format("{0}/{1}/{2}",
                           request[propertyName + ".Month"],
                           request[propertyName + ".Day"],
                           request[propertyName + ".Year"]);

                DateTime dateOfBirth;
                if (DateTime.TryParse(date, out dateOfBirth))
                {
                    base.SetProperty(controllerContext, bindingContext, propertyDescriptor, dateOfBirth);
                    return;
                }
                else
                {
                    bindingContext.ModelState.AddModelError(propertyName, "DateTime was not recognized");
                    return;
                }
            }

            base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
        }
    }
}