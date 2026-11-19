using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Shopping_Cart_System.Domain;
using Shopping_Cart_System.Tests.Arbitraries;

namespace Shopping_Cart_System.Tests.Properties
{
    public class CartProperties
    {
        private static ShoppingCart CloneCart(ShoppingCart source)
        {
            var copy = new ShoppingCart();
            foreach (var item in source.Items)
            {
                copy.Add(new Product(item.Product.Id, item.Product.Name, item.Product.Price), item.Quantity);
            }
            return copy;
        }

        // P2 Інваріант
        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void Cart_invariants_hold_after_add_and_decrease(Product[] products, int quantity)
        {
            var cart = new ShoppingCart();

            foreach (var product in products)
            {
                cart.Add(product, quantity);
                cart.TotalPrice().Should().BeGreaterThanOrEqualTo(0m);
                cart.Items.Should().OnlyContain(i => i.Quantity >= 1);
            }

            if (products.Length > 0 && quantity > 1)
            {
                cart.Decrease(products[0].Id, 1);
                cart.TotalPrice().Should().BeGreaterThanOrEqualTo(0m);
                cart.Items.Should().OnlyContain(i => i.Quantity >= 1);
            }
        }

        // P1 Туди й назад (з передумовою When)
        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public Property Add_then_remove_restores_initial_state(Product[] initialProducts, Product extraProduct, int quantity)
        {
            bool isNewProduct = initialProducts.All(p => p.Id != extraProduct.Id);
            bool ok = true;

            if (isNewProduct)
            {
                var cart = new ShoppingCart();
                foreach (var p in initialProducts)
                {
                    cart.Add(p, 1);
                }

                var initialTotal = cart.TotalPrice();
                var initialCount = cart.Items.Count;

                cart.Add(extraProduct, quantity);
                cart.Remove(extraProduct.Id);

                ok = cart.TotalPrice() == initialTotal
                     && cart.Items.Count == initialCount;
            }

            return ok.When(isNewProduct);
        }

        // P4 Різні шляхи
        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void Two_sequential_adds_equal_single_combined_add(Product[] baseProducts, Product target, int qtyA, int qtyB)
        {
            var baseCart = new ShoppingCart();
            foreach (var p in baseProducts)
            {
                baseCart.Add(p, 1);
            }

            var twoStepsCart = CloneCart(baseCart);
            var singleStepCart = CloneCart(baseCart);

            twoStepsCart.Add(target, qtyA);
            twoStepsCart.Add(target, qtyB);

            singleStepCart.Add(target, qtyA + qtyB);

            twoStepsCart.TotalPrice().Should().Be(singleStepCart.TotalPrice());
            twoStepsCart.Items.Count.Should().Be(singleStepCart.Items.Count);
        }

        // P6 Метаморфічна
        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void Doubling_quantity_doubles_total_price(Product product, int quantity)
        {
            var singleCart = new ShoppingCart();
            var doubleCart = new ShoppingCart();

            singleCart.Add(product, quantity);
            doubleCart.Add(product, quantity * 2);

            doubleCart.TotalPrice().Should().Be(singleCart.TotalPrice() * 2);
        }

        // P1 / P7 Часткове зменшення кількості (з передумовою When)
        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public Property Decrease_partial_amount_when_less_than_added_updates_state(Product product, int addedQty, int decreaseAmount)
        {
            bool applicable = decreaseAmount < addedQty;
            bool ok = true;

            if (applicable)
            {
                var cart = new ShoppingCart();
                cart.Add(product, addedQty);
                var totalBefore = cart.TotalPrice();

                cart.Decrease(product.Id, decreaseAmount);

                var remainingItem = cart.Items.Single(i => i.Product.Id == product.Id);
                ok = remainingItem.Quantity == (addedQty - decreaseAmount)
                     && cart.TotalPrice() <= totalBefore;
            }

            return ok.When(applicable);
        }

        // Перевірка винятку № 2 (ShoppingCart.Remove для відсутнього товару)
        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void Remove_missing_product_throws_InvalidOperationException(Product product)
        {
            var cart = new ShoppingCart();

            Action act = () => cart.Remove(product.Id);

            act.Should().Throw<InvalidOperationException>();
        }

        // Перевірка винятку № 3 (ShoppingCart.Decrease до кількості < 1)
        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void Decrease_below_one_throws_InvalidOperationException(CartItem item)
        {
            var cart = new ShoppingCart();
            cart.Add(item.Product, item.Quantity);

            Action act = () => cart.Decrease(item.Product.Id, item.Quantity);

            act.Should().Throw<InvalidOperationException>();
        }
    }
}