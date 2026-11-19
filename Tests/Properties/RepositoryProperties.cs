using FluentAssertions;
using FsCheck.Xunit;
using Shopping_Cart_System.Domain;
using Shopping_Cart_System.Tests.Arbitraries;

namespace Shopping_Cart_System.Tests.Properties
{
    public class RepositoryProperties
    {
        private static ShoppingCart BuildCart(Product[] products, int quantity)
        {
            var cart = new ShoppingCart();
            foreach (var p in products)
            {
                cart.Add(new Product(p.Id, p.Name, p.Price), quantity);
            }
            return cart;
        }

        // P5 Оракул
        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void Cart_service_save_matches_direct_repository_save(Product[] products, int quantity)
        {
            var repoViaService = new InMemoryCartRepository();
            var service = new CartService(repoViaService);
            var directRepo = new InMemoryCartRepository();

            var cartForService = BuildCart(products, quantity);
            var cartForDirect = BuildCart(products, quantity);

            service.SaveCart(cartForService);
            directRepo.Save(cartForDirect);

            repoViaService.All().Should().HaveCount(directRepo.All().Count);

            var savedViaService = repoViaService.All().Single();
            var savedDirect = directRepo.All().Single();

            savedViaService.TotalPrice().Should().Be(savedDirect.TotalPrice());
            savedViaService.Items.Count.Should().Be(savedDirect.Items.Count);
        }

        // P1 Туди й назад
        [Property(Arbitrary = new[] { typeof(ShopArbitraries) })]
        public void Save_and_all_preserves_cart_total_and_items(Product[] products, int quantity)
        {
            var repository = new InMemoryCartRepository();
            var cart = BuildCart(products, quantity);
            var expectedTotal = cart.TotalPrice();
            var expectedItemsCount = cart.Items.Count;

            repository.Save(cart);
            var allCarts = repository.All();

            allCarts.Should().ContainSingle();
            var loaded = allCarts.Single();
            loaded.TotalPrice().Should().Be(expectedTotal);
            loaded.Items.Count.Should().Be(expectedItemsCount);
        }
    }
}