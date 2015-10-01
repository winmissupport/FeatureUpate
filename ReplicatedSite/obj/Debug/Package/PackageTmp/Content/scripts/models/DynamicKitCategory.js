define(["knockout", "underscore", "models/Item"], function (ko, _, Item) {
    var DynamicKitCategory = (function () {

        function DynamicKitCategory(category) {
            var self                 = this;

            this.CategoryDescription = category.DynamicKitCategoryDescription;
            this.Quantity            = ko.observable(Number(category.Quantity));
            this.Items               = ko.observableArray(_.map(item.DynamicKitCategoryItemMembers, function (dynamicKitCategoryItemMember) {
                return new Item(dynamicKitCategoryItemMember);
            }), this);

            this.maxQuantity         = ko.pureComputed(this._maxQuantity, this);
            this.quantityMultiplier  = ko.observable(0);
            this.selectedQuantity    = ko.pureComputed(this._selectedQuantity, this);
            this.availableQuantity   = ko.pureComputed(this._availableQuantity, this);
            this.validateQuantity    = this._validateQuantity;
            this.selectedItems       = ko.pureComputed(this._selectedItems, this);

            this.instructions        = ko.pureComputed(this._instructions, this);
        }

        DynamicKitCategory.prototype._maxQuantity = function () {
            return Number(this.Quantity()) * Number(this.quantityMultiplier());
        };
        DynamicKitCategory.prototype._selectedQuantity = function () {
            return _.reduce(this.Items(), function (target, item) {
                return target + Number(item.Quantity());
            }, 0);
        };
        DynamicKitCategory.prototype._availableQuantity = function () {
            return Number(this.maxQuantity()) - Number(this.selectedQuantity());
        };
        DynamicKitCategory.prototype._instructions = function () {
            var availableQuantity = Number(this.availableQuantity());
            if (availableQuantity > 0) return "(Choose " + availableQuantity + " more)";
            if (availableQuantity < 0) return "(Take away " + (availableQuantity * -1) + " more)";
            return "";
        };
        DynamicKitCategory.prototype._validateQuantity = function (category, item) {
            var quantity = Number(item.Quantity());
            if (quantity < 0) {
                item.Quantity(0);
                return;
            }

            var availableQuantity = Number(category.availableQuantity());
            if (availableQuantity < 0) {
                item.Quantity(quantity + availableQuantity);
                return;
            }
        };
        DynamicKitCategory.prototype._selectedItems = function () {
            return _.filter(self.Items(), function (item) {
                return item.Quantity() > 0;
            });
        };

        return DynamicKitCategory;
    })();

    return DynamicKitCategory;
});