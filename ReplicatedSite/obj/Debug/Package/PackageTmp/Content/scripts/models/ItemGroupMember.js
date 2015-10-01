define(["knockout", "underscore"], function (ko, _) {

    var ItemGroupMember = (function () {
        function ItemGroupMember(item) {

            this.ItemCode          = item.ItemCode;
            this.MasterItemCode    = item.MasterItemCode;
            this.MemberDescription = item.MemberDescription;
            this.SortOrder         = item.SortOrder;

            this.Item              = ko.observable(item.Item);
        }

        ItemGroupMember.prototype._addQuantity = function () {
            this.Item().Quantity(this.Item().Quantity() + 1);
        };
        ItemGroupMember.prototype._subtractQuantity = function () {
            if (this.Item().Quantity() == 0) return;
            this.Item().Quantity(this.Item().Quantity() - 1);
        };
        ItemGroupMember.prototype._removeItem = function () {
            this.Item().Quantity(0);
        };

        return ItemGroupMember;
    })();

    return ItemGroupMember;
});