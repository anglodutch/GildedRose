using System;
using System.Collections.Generic;
using Xunit;
using GildedRose.Console;

namespace GildedRose.Tests
{
    public class TestAssemblyTests
    {
        [Theory]
        [InlineData("+5 Dexterity Vest", 10, 20, 9, 19)]
        [InlineData("Aged Brie", 2, 0, 1, 1)]
        [InlineData("Elixir of the Mongoose", 5, 7, 4, 6)]
        [InlineData("Sulfuras, Hand of Ragnaros", 0, 80, 0, 80)]//"Sulfuras", being a legendary item, never has to be sold or decreases in Quality
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 15, 20, 14, 21)]
        public void BasicTests(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestScenario(itemName, sellInStart, qualityStart, expectedSellIn, expectedQuality);
        }

        private static void TestScenario(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            var p = new Program
            {
                Items = new List<Item>
                {
                    new Item {Name = itemName, SellIn = sellInStart, Quality = qualityStart},
                }
            };
            p.UpdateQuality();

            //Aged Brie", SellIn = 2, Quality = 0},
            Assert.Equal(expectedSellIn, p.Items[0].SellIn);
            Assert.Equal(expectedQuality, p.Items[0].Quality);
        }

        [Theory]
        //Once the sell by date has passed, Quality degrades twice as fast
        [InlineData("+5 Dexterity Vest", 1, 20, 0, 19)]
        [InlineData("Elixir of the Mongoose", 1, 7, 0, 6)]
        [InlineData("+5 Dexterity Vest", 0, 20, -1, 18)]
        [InlineData("Elixir of the Mongoose", 0, 7, -1, 5)]
        public void QualityDegradingAfterSellByTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestScenario(itemName, sellInStart, qualityStart, expectedSellIn, expectedQuality);
        }


        [Theory]
        //The Quality of an item is never negative
        [InlineData("+5 Dexterity Vest", 2, 1, 1, 0)]
        [InlineData("Elixir of the Mongoose", 2, 1, 1, 0)]
        [InlineData("+5 Dexterity Vest", 2, 0, 1, 0)]
        [InlineData("Elixir of the Mongoose", 2, 0, 1, 0)]
        public void QualityNotNegativeTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestScenario(itemName, sellInStart, qualityStart, expectedSellIn, expectedQuality);
        }

        [Theory]
        //The Quality of an item is never more than 50
        [InlineData("Aged Brie", 2, 49, 1, 50)]
        [InlineData("Aged Brie", 2, 50, 1, 50)]
        [InlineData("Aged Brie", 1, 25, 0, 26)]
        [InlineData("Aged Brie", 0, 25, -1, 27)]
        public void BrieTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestScenario(itemName, sellInStart, qualityStart, expectedSellIn, expectedQuality);
        }

        [Theory]
        // baskstage passes, test they never go above 50 and check that the change in incriment is applied in the correct places
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 11, 49, 10, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 11, 50, 10, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 10, 49, 9, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 10, 48, 9, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 10, 50, 9, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 5, 50, 4, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 5, 49, 4, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 5, 48, 4, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 5, 47, 4, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 1, 5, 0, 8)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 0, 5, -1, 0)]
        public void BackstagePassTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestScenario(itemName, sellInStart, qualityStart, expectedSellIn, expectedQuality);
        }
        //"Backstage passes", like aged brie, increases in Quality as it's SellIn value approaches; Quality increases by 2 when there are 10 days or less and by 3 when there are 5 days or less but Quality drops to 0 after the concert

        //These tests initally fail as there is no handlig currently for Conjured Items
        [Theory]
        //"Conjured" items degrade in Quality twice as fast as normal items
        [InlineData("Conjured Mana Cake", 3, 6, 2, 4)]
        //Once the sell by date has passed, Quality degrades twice as fast
        [InlineData("Conjured Mana Cake", 1, 6, 0, 4)]
        [InlineData("Conjured Mana Cake", 0, 6, -1, 2)]
        //The Quality of an item is never negative
        [InlineData("Conjured Mana Cake", 2, 2, 1, 0)]
        [InlineData("Conjured Mana Cake", 2, 1, 1, 0)]
        [InlineData("Conjured Mana Cake", 2, 0, 1, 0)]
        public void ConjuredItemTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestScenario(itemName, sellInStart, qualityStart, expectedSellIn, expectedQuality);
        }

        [Theory]
        [InlineData("+5 Dexterity Vest", typeof(UpdateItem))]
        [InlineData("Aged Brie", typeof(MaturingItem))]
        [InlineData("Elixir of the Mongoose", typeof(UpdateItem))]
        [InlineData("Sulfuras, Hand of Ragnaros", typeof(LegendaryItem))]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", typeof(BackstagePass))]
        [InlineData("Backstage passes to a Queen concert", typeof(BackstagePass))]
        [InlineData("Conjured Mana Cake", typeof(ConjuredItem))]
        [InlineData("Conjured SPAM", typeof(ConjuredItem))]
        public void SimpleFactoryTests(string name, Type expectedType)
        {
            IUpdateItemFactory factory = new SimpleUpdateItemFactory();

            var updateItem = factory.GetUpdateItem(new Item { Name = name });

            Assert.IsType(expectedType, updateItem);
        }

        [Theory]
        [InlineData("+5 Dexterity Vest", typeof(UpdateItem))]
        [InlineData("Aged Brie", typeof(MaturingItem))]
        [InlineData("Elixir of the Mongoose", typeof(UpdateItem))]
        [InlineData("Sulfuras, Hand of Ragnaros", typeof(LegendaryItem))]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", typeof(BackstagePass))]
        [InlineData("Backstage passes to a Queen concert", typeof(BackstagePass))]
        [InlineData("Conjured Mana Cake", typeof(ConjuredItem))]
        [InlineData("Conjured SPAM", typeof(ConjuredItem))]
        public void DynamicFactoryTests(string name, Type expectedType)
        {
            IUpdateItemFactory factory = new DynamicUpdateItemFactory();

            var updateItem = factory.GetUpdateItem(new Item { Name = name });

            Assert.IsType(expectedType, updateItem);
        }
    }

    public class ItemTestBase
    {
        protected void TestItem(UpdateItem updateItem, int expectedSellIn, int expectedQuality)
        {
            updateItem.Update();

            Assert.Equal(expectedSellIn, updateItem.Item.SellIn);
            Assert.Equal(expectedQuality, updateItem.Item.Quality);
        }
    }

    public class UpdateItemTest : ItemTestBase
    {
        [Theory]
        [InlineData("+5 Dexterity Vest", 10, 20, 9, 19)]

        public void NotingChangesTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestItem(new UpdateItem(new Item { Name = itemName, SellIn = sellInStart, Quality = qualityStart }), expectedSellIn, expectedQuality);
        }

        [Theory]
        //Once the sell by date has passed, Quality degrades twice as fast
        [InlineData("+5 Dexterity Vest", 1, 20, 0, 19)]
        [InlineData("Elixir of the Mongoose", 1, 7, 0, 6)]
        [InlineData("+5 Dexterity Vest", 0, 20, -1, 18)]
        [InlineData("Elixir of the Mongoose", 0, 7, -1, 5)]
        public void SelinZeroDegradeTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestItem(new UpdateItem(new Item { Name = itemName, SellIn = sellInStart, Quality = qualityStart }), expectedSellIn, expectedQuality);
        }
        [Theory]
        //The Quality of an item is never negative
        [InlineData("+5 Dexterity Vest", 2, 1, 1, 0)]
        [InlineData("Elixir of the Mongoose", 2, 1, 1, 0)]
        [InlineData("+5 Dexterity Vest", 2, 0, 1, 0)]
        [InlineData("Elixir of the Mongoose", 2, 0, 1, 0)]
        public void QualityIsNotNegative(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestItem(new UpdateItem(new Item { Name = itemName, SellIn = sellInStart, Quality = qualityStart }), expectedSellIn, expectedQuality);
        }
    }

    public class LegendaryItemTest : ItemTestBase
    {
        [Theory]
        [InlineData("Sulfuras, Hand of Ragnaros", 0, 80, 0, 80)]
        [InlineData("Sulfuras, Hand of Ragnaros", 5, 80, 5, 80)]
        public void NotingChangesTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestItem(new LegendaryItem(new Item { Name = itemName, SellIn = sellInStart, Quality = qualityStart }), expectedSellIn, expectedQuality);
        }
    }

    public class ConjuredItemTest : ItemTestBase
    {
        [Theory]
        //The Quality of an item is never negative
        [InlineData("Conjured Mana Cake", 2, 2, 1, 0)]
        [InlineData("Conjured Mana Cake", 2, 1, 1, 0)]
        [InlineData("Conjured Mana Cake", 2, 0, 1, 0)]
        public void QualityNotNegativeTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestItem(new ConjuredItem(new Item { Name = itemName, SellIn = sellInStart, Quality = qualityStart }), expectedSellIn, expectedQuality);
        }
        [Theory]
        //"Conjured" items degrade in Quality twice as fast as normal items
        [InlineData("Conjured Mana Cake", 3, 6, 2, 4)]
        public void DegradeTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestItem(new ConjuredItem(new Item { Name = itemName, SellIn = sellInStart, Quality = qualityStart }), expectedSellIn, expectedQuality);
        }
        [Theory]
        //Once the sell by date has passed, Quality degrades twice as fast
        [InlineData("Conjured Mana Cake", 1, 6, 0, 4)]
        [InlineData("Conjured Mana Cake", 0, 6, -1, 2)]
        public void SelinZeroDegradeTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestItem(new ConjuredItem(new Item { Name = itemName, SellIn = sellInStart, Quality = qualityStart }), expectedSellIn, expectedQuality);
        }
    }

    public class BackstagePassItemTest : ItemTestBase
    {
        [Theory]
        // baskstage passes, test they never go above 50 and check that the change in incriment is applied in the correct places
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 11, 49, 10, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 11, 50, 10, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 10, 49, 9, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 10, 48, 9, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 10, 50, 9, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 5, 50, 4, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 5, 49, 4, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 5, 48, 4, 50)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 5, 47, 4, 50)]
        public void QualityNotAboveFiftyTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestItem(new BackstagePass(new Item { Name = itemName, SellIn = sellInStart, Quality = qualityStart }), expectedSellIn, expectedQuality);
        }
        [Theory]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 1, 5, 0, 8)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 0, 5, -1, 0)]
        public void QualityToZeroOnExpiry(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestItem(new BackstagePass(new Item { Name = itemName, SellIn = sellInStart, Quality = qualityStart }), expectedSellIn, expectedQuality);
        }
        [Theory]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 11, 5, 10, 6)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 10, 5, 9, 7)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 6, 5, 5, 7)]
        [InlineData("Backstage passes to a TAFKAL80ETC concert", 5, 5, 4, 8)]
        public void QualityIncreaseMoreRapidlyAsSellinDecrease(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestItem(new BackstagePass(new Item { Name = itemName, SellIn = sellInStart, Quality = qualityStart }), expectedSellIn, expectedQuality);
        }
    }

    public class MaturingtemTest : ItemTestBase
    {
        [Theory]
        //The Quality of an item is never more than 50
        [InlineData("Aged Brie", 2, 49, 1, 50)]
        [InlineData("Aged Brie", 2, 50, 1, 50)]
        [InlineData("Aged Brie", 1, 25, 0, 26)]
        [InlineData("Aged Brie", 0, 25, -1, 27)]
        public void QualityNotAboveFiftyTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestItem(new MaturingItem(new Item { Name = itemName, SellIn = sellInStart, Quality = qualityStart }), expectedSellIn, expectedQuality);
        }

        [Theory]
        [InlineData("Aged Brie", 1, 25, 0, 26)]
        [InlineData("Aged Brie", 0, 25, -1, 27)]
        public void QualityIncreasesMoreRapidlyAfterSellinTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestItem(new MaturingItem(new Item { Name = itemName, SellIn = sellInStart, Quality = qualityStart }), expectedSellIn, expectedQuality);
        }

        [Theory]
        [InlineData("Aged Brie", 2, 0, 1, 1)]
        [InlineData("Aged Brie", 3, 0, 2, 1)]
        public void QualityIncreasesTest(string itemName, int sellInStart, int qualityStart, int expectedSellIn, int expectedQuality)
        {
            TestItem(new MaturingItem(new Item { Name = itemName, SellIn = sellInStart, Quality = qualityStart }), expectedSellIn, expectedQuality);
        }
    }
}