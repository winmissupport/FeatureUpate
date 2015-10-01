define(["knockout", "underscore", "models/DynamicKitCategory"], function (ko, _, DynamicKitCategory) {
    var DynamicKit = (function () {
        function DynamicKit(item) {

            this.ItemCode                 = item.ItemCode;
            this.Quantity                 = ko.observable(item.Quantity);
            this.ParentItemCode           = item.ParentItemCode;
            this.GroupMasterItemCode      = item.GroupMasterItemCode;
            this.DynamicKitCategory       = item.DynamicKitCategory;
            this.Type                     = item.Type;

            this.Item                     = item;

            this.IsDynamicKitMaster       = item.IsDynamicKitMaster;
            this.DynamicKitCategories = ko.observableArray(_.map(item.DynamicKitCategories, function (dynamicKitCategory) {
                return new DynamicKitCategory(dynamicKitCategory);
            }), this);;
            this.DynamicKitMasterItemCode = item.DynamicKitMasterItemCode;

            // Methods
            this.editor                   = this._editor;
            this.formattedPrice           = ko.pureComputed(this._formattedPrice, this);
            this.addQuantity              = this._addQuantity;
            this.subtractQuantity         = this._subtractQuantity;
            this.removeItem               = this._removeItem;
            this.subtotal                 = ko.computed(this._subtotal, this);
            this.customizeKit             = this._customizeKit;
            this.selectedDynamicKitItems  = ko.computed(this._selectedDynamicKitItems, this);
            this.cartItems                = ko.computed(this._cartItems, this);
        }

        DynamicKit.prototype._editor = "DynamicKit";
        DynamicKit.prototype._formattedPrice = function () {
            return '$' + this.Price;
        };
        DynamicKit.prototype._addQuantity = function () {
            this.Quantity(Number(this.Quantity()) + 1);
        };
        DynamicKit.prototype._subtractQuantity = function () {
            if (Number(this.Quantity()) == 0) return;
            this.Quantity(Number(this.Quantity()) - 1);
        };
        DynamicKit.prototype._removeItem = function () {
            this.Quantity(0);
        };
        DynamicKit.prototype._subtotal = function () {
            return this.Price * this.Quantity();
        };
        DynamicKit.prototype._cartItems = function () {
            var result = [];

            if (Number(this.Quantity()) > 0) result.push(this);

            return result;
        };
        DynamicKit.prototype._customizeKit = function () {
            this.Quantity(1);
        };
        DynamicKit.prototype._selectedDynamicKitItems = function () {
            var result = [];

            if (this.IsDynamicKitMaster && this.Quantity() > 0) {
                var isValid = true;
                _.each(this.DynamicKitCategories(), function (category) {
                    if (Number(category.availableQuantity()) != 0) {
                        isValid = false;
                    }
                });

                if (isValid) {
                    result.push(this);
                    var items = _.each(this.DynamicKitCategories(), function (category) {
                        var items = category.Items();
                        var cartItems = _.filter(items, function (item) {
                            return Number(item.Quantity()) > 0;
                        });
                        result = _.union(result, cartItems);
                    });
                }
            }

            return result;
        };

        return DynamicKit;
    })();

    return DynamicKit;
});