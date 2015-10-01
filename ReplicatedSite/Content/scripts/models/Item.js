define(["knockout", "underscore"], function (ko, _) {
    var Item = (function () {
        function Item(item) {

            this.ItemCode            = item.ItemCode;
            this.Quantity            = ko.observable(item.Quantity);
            this.ParentItemCode      = item.ParentItemCode;
            this.GroupMasterItemCode = item.GroupMasterItemCode;
            this.DynamicKitCategory  = item.DynamicKitCategory;
            this.Type                = item.Type;

            this.Item = item;

            // Methods
            this.editor                  = this._editor;
            this.formattedPrice          = ko.pureComputed(this._formattedPrice, this);
            this.addQuantity             = this._addQuantity;
            this.subtractQuantity        = this._subtractQuantity;
            this.removeItem              = this._removeItem;
            this.subtotal                = ko.computed(this._subtotal, this);
            this.cartItems               = ko.computed(this._cartItems, this);
        }

        Item.prototype._editor = "Item";
        Item.prototype._formattedPrice = function () {
            return '$' + this.Item.Price;
        };
        Item.prototype._addQuantity = function () {
            this.Quantity(Number(this.Quantity()) + 1);
        };
        Item.prototype._subtractQuantity = function () {
            if (Number(this.Quantity()) == 0) return;
            this.Quantity(Number(this.Quantity()) - 1);
        };
        Item.prototype._removeItem = function () {
            this.Quantity(0);
        };
        Item.prototype._subtotal = function () {
            return this.Item.Price * this.Quantity();
        };
        Item.prototype._cartItems = function () {
            var result = [];

            if (Number(this.Quantity()) > 0) result.push(this);

            return result;
        };

        return Item;
    })();

    return Item;
});