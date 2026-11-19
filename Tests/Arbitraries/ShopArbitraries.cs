using FsCheck;
using FsCheck.Fluent;
using Shopping_Cart_System.Domain;

namespace Shopping_Cart_System.Tests.Arbitraries
{
    public static class ShopArbitraries
    {
        private static readonly string[] SampleNames =
        {
            "Apple", "Milk", "Bread", "Coffee", "Cheese", "Tea", "Chocolate"
        };

        private static Gen<string> NonEmptyAlphaString()
        {
            var idGen = Gen.Choose(1, 10).Select(n => $"P{n}");
            var nameGen = Gen.Elements(SampleNames);
            return Gen.OneOf(idGen, nameGen);
        }

        public static Arbitrary<Product> Product()
        {
            var priceGen = Gen.Choose(0, 100_000).Select(c => c / 100m);

            var gen = from id in NonEmptyAlphaString()
                from name in NonEmptyAlphaString()
                from price in priceGen
                select new Product(id, name, price);

            return Arb.From(gen);
        }

        public static Arbitrary<int> Quantity()
        {
            var gen = Gen.OneOf(
                Gen.Constant(1),
                Gen.Choose(1, 50)
            );

            return Arb.From(gen);
        }

        public static Arbitrary<CartItem> CartItem()
        {
            var gen = from product in Product().Generator
                from quantity in Quantity().Generator
                select new CartItem(product, quantity);

            return Arb.From(gen);
        }
    }
}