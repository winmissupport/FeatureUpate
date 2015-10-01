define(["knockout", "underscore", "models/ItemGroupMember"], function (ko, _, ItemGroupMember) {
    var ItemGroup = (function () {
        function ItemGroup(item) {

            this.GroupedItemCode            = item.GroupedItemCode;
            this.Quantity                   = ko.observable(item.Quantity);
            this.ParentGroupedItemCode      = item.ParentGroupedItemCode;
            this.GroupMasterGroupedItemCode = item.GroupMasterGroupedItemCode;
            this.DynamicKitCategory         = item.DynamicKitCategory;
            this.Type                       = item.Type;

            this.GroupMembers               = ko.observableArray(_.map(item.GroupMembers, function(groupMember) {
                return new ItemGroupMember(groupMember);
            }), this);
            this.GroupMasterItemDescription = item.GroupMasterItemDescription;

            this.Item                       = item;

            // Methods
            this.editor                     = this._editor;
            this.formattedPrice             = ko.pureComputed(this._formattedPrice, this);
            this.addQuantity                = this._addQuantity;
            this.subtractQuantity           = this._subtractQuantity;
            this.removeGroupedItem          = this._removeGroupedItem;
            this.subtotal                   = ko.computed(this._subtotal, this);
            this.cartItems                  = ko.computed(this._cartItems, this);
        }

        ItemGroup.prototype._editor = "GroupedItem";
        ItemGroup.prototype._formattedPrice = function () {
            return '$' + this.Item.Price;
        };
        ItemGroup.prototype._addQuantity = function () {
            this.Quantity(Number(this.Quantity()) + 1);
        };
        ItemGroup.prototype._subtractQuantity = function () {
            if (Number(this.Quantity()) == 0) return;
            this.Quantity(Number(this.Quantity()) - 1);
        };
        ItemGroup.prototype._removeItem = function () {
            this.Quantity(0);
        };
        ItemGroup.prototype._subtotal = function () {
            return this.Item.Price * this.Quantity();
        };
        ItemGroup.prototype._cartItems = function () {
            var result = [];

            _.each(this.GroupMembers(), function (groupMember) {
                if (Number(groupMember.Item().Quantity()) > 0) result.push(groupMember.Item());
            });

            return result;
        };

        return ItemGroup;
    })();

    return ItemGroup;
});