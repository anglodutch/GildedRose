using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GildedRose.Console
{
    public class Program
    {
        public IList<Item> Items;
        static void Main(string[] args)
        {
            System.Console.WriteLine("OMGHAI!");

            var app = new Program()
                          {
                              Items = new List<Item>
                                          {
                                              new Item {Name = "+5 Dexterity Vest", SellIn = 10, Quality = 20},
                                              new Item {Name = "Aged Brie", SellIn = 2, Quality = 0},
                                              new Item {Name = "Elixir of the Mongoose", SellIn = 5, Quality = 7},
                                              new Item {Name = "Sulfuras, Hand of Ragnaros", SellIn = 0, Quality = 80},
                                              new Item
                                                  {
                                                      Name = "Backstage passes to a TAFKAL80ETC concert",
                                                      SellIn = 15,
                                                      Quality = 20
                                                  },
                                              new Item {Name = "Conjured Mana Cake", SellIn = 3, Quality = 6}
                                          }

                          };

            app.UpdateQuality();

            System.Console.ReadKey();

        }

        public void UpdateQuality()
        {
            IUpdateItemFactory itemFactory = new DynamicUpdateItemFactory();
            foreach (var updateItem in Items.Select(itemFactory.GetUpdateItem))
            {
                updateItem.Update();
            }
        }
    }

    public class Item
    {
        public string Name { get; set; }

        public int SellIn { get; set; }

        public int Quality { get; set; }
    }

    public interface IUpdateItemFactory
    {
        UpdateItem GetUpdateItem(Item item);
    }



    /// <summary>
    /// This simple factory would require more maintenance if the were to be more types added in the future
    /// However it is quite simple.
    /// </summary>
    public class SimpleUpdateItemFactory : IUpdateItemFactory
    {
        public UpdateItem GetUpdateItem(Item item)
        {
            if (item.Name.StartsWith("Conjured"))// The spec for the new functionality mentions jsut Conjured items so we can assume that there may be types other than 'Conjured Mana Cake'. If I was doing this for real I would seek clarification on this aspect. 
            {
                return new ConjuredItem(item);
            }
            if (item.Name.StartsWith("Backstage passes"))// I've done this using a StartsWith on the basis that there might be other concerts and therefore it removed the need to add more spacific names of items that would need maintenance
            {
                return new BackstagePass(item);
            }
            if (item.Name == "Sulfuras, Hand of Ragnaros")
            {
                return new LegendaryItem(item);
            }
            if (item.Name == "Aged Brie")
            {
                return new MaturingItem(item);
            }
            return new UpdateItem(item);
        }
    }

    /// <summary>
	/// If required we could use this Factory which will dynamically load special Items and would therefore allow the solution to be 
	/// extensible based ont
	/// </summary>
	public class DynamicUpdateItemFactory : IUpdateItemFactory
    {
        private readonly List<Tuple<List<string>, Type>> _specialTypes = new List<Tuple<List<string>, Type>>();

        public DynamicUpdateItemFactory()
        {
            LoadTypes();
        }

        private void LoadTypes()
        {
            foreach (var type in GetType().Assembly.CreatableTypes<UpdateItem>())
            {
                var attribute = type.GetCustomAttributes<SpecialUpdateItemAttribute>().ToList();
                if (attribute.Any())
                {
                    _specialTypes.Add(new Tuple<List<string>, Type>(attribute.Select(a => a.Prefix).ToList(), type));
                }
            }
        }

        public UpdateItem GetUpdateItem(Item item)
        {
            foreach (var specialType in _specialTypes)
            {
                if (specialType.Item1.Any(prefix => item.Name.StartsWith(prefix)))
                {
                    return Activator.CreateInstance(specialType.Item2, item) as UpdateItem;
                }
            }
            return new UpdateItem(item);
        }
    }

    public static class ExtensionMethods
    {
        public static IEnumerable<Type> CreatableTypes<T>(this Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes.Select(t => t.AsType())
                .Select(t => t.GetTypeInfo())
                .Where(t => !t.IsAbstract && t.DeclaredConstructors.Any(c => !c.IsStatic && c.IsPublic) && t.IsSubclassOf(typeof(T))).Select(t => t.AsType());
            }
            catch (ReflectionTypeLoadException)
            {
                return new Type[0];
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SpecialUpdateItemAttribute : Attribute
    {
        public SpecialUpdateItemAttribute(string prefix)
        {
            Prefix = prefix;
        }

        public string Prefix { get; }
    }

    public class UpdateItem
    {
        protected int QualityDelta { get; set; } = 1;

        public UpdateItem(Item item)
        {
            Item = item;
        }

        public Item Item { get; }

        public virtual void Update()
        {
            UpdateQuality();
            UpdateSellIn();
        }

        protected virtual void UpdateQuality()
        {
            UpdateQuality(Item.SellIn <= 0 ? QualityDelta * 2 : QualityDelta);
        }

        protected virtual void UpdateQuality(int delta)
        {
            if (Item.Quality < delta)
            {
                Item.Quality = 0;
            }
            else
            {
                Item.Quality -= delta;
            }
        }

        private void UpdateSellIn()
        {
            Item.SellIn--;
        }
    }

    /// <summary>
    /// Maturing Items get better with age. 
    /// </summary>
	[SpecialUpdateItem("Aged Brie")]
    public class MaturingItem : UpdateItem
    {
        public MaturingItem(Item item) : base(item)
        {

        }

        protected override void UpdateQuality(int delta)
        {
            if (Item.Quality + delta >= 50)
            {
                Item.Quality = 50;
            }
            else
            {
                Item.Quality += delta;
            }
        }
    }

    /// <summary>
    /// These items do not degrade and do not need to be sold therefore the Update method does nothing
    /// </summary>
	[SpecialUpdateItem("Sulfuras, Hand of Ragnaros")]
    public class LegendaryItem : UpdateItem
    {
        public LegendaryItem(Item item) : base(item)
        {
            QualityDelta = 0;
        }

        public override void Update()
        {
            // do nothing for Legendary Items
        }
    }

	[SpecialUpdateItem("Backstage passes")]
    public class BackstagePass : MaturingItem
    {
        public BackstagePass(Item item) : base(item)
        {

        }

        protected override void UpdateQuality()
        {
            //We could use a state pattern here but for now I have left it as a series of if statements

            if (Item.SellIn <= 0)
            {
                Item.Quality = 0;
                return;
            }

            //could do this a boundary check but would then also need to do it in the constructor to ensure it's initalise correctly.
            if (Item.SellIn <= 5)
            {
                QualityDelta = 3;
            }
            else if (Item.SellIn <= 10)
            {
                QualityDelta = 2;
            }
            else
            {
                QualityDelta = 1;
            }
            base.UpdateQuality();
        }
    }

	[SpecialUpdateItem("Conjured")]
    public class ConjuredItem : UpdateItem
    {
        public ConjuredItem(Item item) : base(item)
        {
            QualityDelta = 2;
        }
    }
}
