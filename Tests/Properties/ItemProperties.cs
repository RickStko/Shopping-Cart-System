using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Shopping_Cart_System.Domain;
using Shopping_Cart_System.Tests.Arbitraries;

namespace Shopping_Cart_System.Tests.Properties
{
    public class ItemProperties
    {
        // P8 Без винятків
        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void Create_and_increase_cart_item_does_not_throw(Product product, PositiveInt initialQty, PositiveInt extraQty)
        {
            Action act = () =>
            {
                var item = new CartItem(product, initialQty.Get);
                item.Increase(extraQty.Get);
            };

            act.Should().NotThrow();
        }

        // P7 Легко перевірити
        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void Initial_quantity_is_recoverable_after_increase(CartItem item, int extra)
        {
            var initialQty = item.Quantity;

            item.Increase(extra);
            var recoveredInitialQty = item.Quantity - extra;

            recoveredInitialQty.Should().Be(initialQty);
        }

        // Перевірка винятку № 1 (CartItem.Increase з недодатною кількістю кидає InvalidOperationException)
        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void Increase_non_positive_amount_throws_InvalidOperationException(CartItem item, NonNegativeInt invalidAmount)
        {
            Action act = () => item.Increase(-invalidAmount.Get);

            act.Should().Throw<InvalidOperationException>();
        }
    }
}