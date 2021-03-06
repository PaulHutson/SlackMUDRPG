﻿using System;
using System.Collections.Generic;
using System.Linq;
using SlackMUDRPG.Utility.Formatters;

namespace SlackMUDRPG.CommandClasses
{
	public static class SMItemHelper
	{
		/// <summary>
		/// Gets an SMitem from a list of items based on a given item identifier.
		/// </summary>
		/// <returns>SMItem found in the list</returns>
		/// <param name="list">List of SMItems to look in.</param>
		/// <param name="itemIdentifier">Item identifier (id, name, familiy).</param>
		/// <param name="recursive">If set to <c>true</c> recursive search through container items in the list.</param>
		public static SMItem GetItemFromList(List<SMItem> list, string itemIdentifier, bool recursive = true)
		{
			if (list == null)
			{
				return null;
			}

			if (recursive == true)
			{
				return FindItemInListRecursive(list, itemIdentifier);
			}

			return FindItemInList(list, itemIdentifier);
		}

		/// <summary>
		/// Removes anSMItem from a list of items based on a given item identifier.
		/// </summary>
		/// <returns><c>true</c>, if the was removed, <c>false</c> otherwise.</returns>
		/// <param name="list">List of SMItems to remove from.</param>
		/// <param name="itemIdentifier">Item identifier (id, name, family).</param>
		/// <param name="recursive">If set to <c>true</c> recursive look through container items in the list until
		/// one item is removed.</param>
		public static bool RemoveItemFromList(List<SMItem> list, string itemIdentifier, bool recursive = true)
		{
			if (list == null)
			{
				return false;
			}

			if (recursive == true)
			{
				return RemoveItemFromListRecursive(list, itemIdentifier);
			}

			SMItem itemToRemove = GetItemFromList(list, itemIdentifier, false);
			if (itemToRemove != null)
			{
				list.Remove(itemToRemove);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the item weight, including any items it contains.
		/// </summary>
		/// <returns>The item weight.</returns>
		/// <param name="item">Item to get the weight for.</param>
		public static int GetItemWeight(SMItem item)
		{
			int weight = item.ItemWeight;

			if (item.CanHoldOtherItems() && item.HeldItems != null)
			{
				foreach (SMItem smi in item.HeldItems)
				{
					weight += GetItemWeight(smi);
				}
			}

			return weight;
		}

		/// <summary>
		/// Calculates the available capacity of a container.
		/// </summary>
		/// <param name="item">The container to look at.</param>
		/// <returns>The containers available capacity.</returns>
		public static int GetItemAvailbleCapacity(SMItem item)
		{
			if (!item.CanHoldOtherItems())
			{
				return 0;
			}

			if (item.HeldItems == null || !item.HeldItems.Any())
			{
				return item.ItemCapacity;
			}

			int used = 0;

			if (item.HeldItems.Any())
			{
				foreach (SMItem smi in item.HeldItems)
				{
					used += smi.ItemSize;
				}
			}

			return item.ItemCapacity - used;

		}

		/// <summary>
		/// Calculates the used capacity of a container.
		/// </summary>
		/// <param name="item">The container to look at.</param>
		/// <returns>The containers used capacity.</returns>
		public static int GetItemUsedCapacity(SMItem item)
		{
			if (!item.CanHoldOtherItems())
			{
				return 0;
			}

			int used = 0;

			if (item.HeldItems.Any())
			{
				foreach (SMItem smi in item.HeldItems)
				{
					used += smi.ItemSize;
				}
			}

			return used;

		}

		/// <summary>
		/// Checks if a given item matches a given string identifier (id, name, family).
		/// </summary>
		/// <returns><c>true</c>, if the identifier matches the itemed, <c>false</c> otherwise.</returns>
		/// <param name="item">SMItem.</param>
		/// <param name="identifier">String identifier (id, name, family).</param>
		public static bool ItemMatches(SMItem item, string identifier)
		{
			string ciIdentifier = identifier.ToLower();

			if (item.ItemID == identifier ||
				item.ItemName.ToLower() == ciIdentifier ||
				item.PluralName.ToLower() == ciIdentifier ||
				item.ItemFamily.ToLower() == ciIdentifier ||
				item.GetPluralFamilyName().ToLower() == ciIdentifier ||
				item.ItemType.ToLower() == ciIdentifier)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the items listing of a given container in a string ready for outputting.
		/// </summary>
		/// <param name="container">Container SMItem to get the listings for.</param>
		/// <returns>Containers item listings.</returns>
		public static string GetContainerContents(SMItem container)
		{
			// Return null it not a container
			if (!container.CanHoldOtherItems())
			{
				return null;
			}

			// If the container is empty
			if (container.HeldItems == null || !container.HeldItems.Any())
			{
				return ResponseFormatterFactory.Get().Italic("Empty", 1);
			}

			// Get list of item counts
			// TODO bring this functionality into this class
			List<ItemCountObject> lines = SMItemUtils.GetItemCountList(container.HeldItems);

			string output = "";

			foreach (ItemCountObject line in lines)
			{
				string itemDetails = $"{line.Count} x ";

				if (line.Count > 1)
				{
					itemDetails += line.PluralName;
				}
				else
				{
					itemDetails += line.SingularName;
				}

				output += ResponseFormatterFactory.Get().ListItem(itemDetails);
			}

			return output;
		}

		/// <summary>
		/// Puts an item in a container, initialising the containers HeldItems property to an new list if required.
		/// </summary>
		/// <param name="item">The item to put.</param>
		/// <param name="container">The container to put in.</param>
		public static void PutItemInContainer(SMItem item, SMItem container)
		{
			if (container.HeldItems == null)
			{
				container.HeldItems = new List<SMItem>();
			}

			container.HeldItems.Add(item);
		}

		/// <summary>
		/// Counts the number of items in a given container that match a given id, name or family.
		/// </summary>
		/// <param name="container">The countainer to look in.</param>
		/// <param name="itemIdentiifier">The id, name or family to match.</param>
		/// <returns>The count of matching items.</returns>
		public static int CountItemsInContainer(SMItem container, string itemIdentiifier)
		{
			int count = 0;

			if (container.HeldItems != null && container.HeldItems.Any())
			{
				foreach (SMItem item in container.HeldItems)
				{
					if (ItemMatches(item, itemIdentiifier))
					{
						count++;
					}
				}
			}

			return count;
		}

		/// <summary>
		/// Searches a specified container recursively to find a suitable container to place a given item in.
		/// Will return the specified container itself if the item can go directly in to it.
		/// </summary>
		/// <param name="item">The item to find a container for.</param>
		/// <param name="container">The container to search in.</param>
		/// <returns>An SMItem object the item can go in or null.</returns>
		public static SMItem FindContainerForItemRecursive(SMItem item, SMItem container)
		{
			SMItem foundContainer = container.CanHoldItem(item) ? container : null;

			if (foundContainer == null)
			{
				foreach (SMItem innerItem in container.HeldItems)
				{
					if (innerItem.CanHoldOtherItems() && innerItem.HeldItems != null)
					{
						foundContainer = FindContainerForItemRecursive(item, innerItem);

						if (foundContainer != null)
						{
							return foundContainer;
						}
					}
				}
			}

			return foundContainer;
		}

		/// <summary>
		/// Searches a specified container recursively to find a container matching a given identifier.
		/// Will return the specified container itself if the identifier matches.
		/// </summary>
		/// <param name="identifier">The identifier for the container to search for.</param>
		/// <param name="container">The container to search in.</param>
		/// <returns>An SMItem object representing the container matching the idetifier.</returns>
		public static SMItem FindContainerInContainerRecursive(string identifier, SMItem container)
		{
			SMItem foundContainer = ItemMatches(container, identifier) ? container : null;

			if (foundContainer == null)
			{
				foreach (SMItem innerItem in container.HeldItems)
				{
					if (innerItem.CanHoldOtherItems())
					{
						if (ItemMatches(innerItem, identifier))
						{
							return innerItem;
						}

						return FindContainerInContainerRecursive(identifier, innerItem);
					}
				}
			}

			return foundContainer;
		}

		/// <summary>
		/// Finds an item in a list recursivly looking through containers within containers.
		/// </summary>
		/// <returns>The item from the list.</returns>
		/// <param name="list">List.</param>
		/// <param name="itemIdentifier">Item identifier.</param>
		private static SMItem FindItemInListRecursive(List<SMItem> list, string itemIdentifier)
		{
			SMItem foundItem = FindItemInList(list, itemIdentifier);

			if (foundItem == null)
			{
				foreach (SMItem item in list)
				{
					// TODO account for locked containers

					if (item.CanHoldOtherItems() && item.HeldItems != null)
					{
						foundItem = FindItemInListRecursive(item.HeldItems, itemIdentifier);
						if (foundItem != null)
						{
							return foundItem;
						}
					}
				}
			}

			return foundItem;
		}

		/// <summary>
		/// Finds an item in a list by matching the items id, name or family to a given identifier.
		/// </summary>
		/// <returns>The item from the list.</returns>
		/// <param name="list">List od items to look search.</param>
		/// <param name="itemIdentifier">Item identifier.</param>
		private static SMItem FindItemInList(List<SMItem> list, string itemIdentifier)
		{
			string ciIdentifier = itemIdentifier.ToLower();

			SMItem foundItem = null;

			foundItem = list.FirstOrDefault((SMItem item) => item.ItemID == itemIdentifier);

			if (foundItem == null)
			{
				foundItem = list.FirstOrDefault((SMItem item) => item.ItemName.ToLower() == ciIdentifier);
			}

			if (foundItem == null)
			{
				foundItem = list.FirstOrDefault((SMItem item) => item.PluralName.ToLower() == ciIdentifier);
			}

			if (foundItem == null)
			{
				foundItem = list.FirstOrDefault((SMItem item) => item.ItemFamily.ToLower() == ciIdentifier);
			}

			if (foundItem == null)
			{
				foundItem = list.FirstOrDefault((SMItem item) => item.GetPluralFamilyName().ToLower() == ciIdentifier);
			}

			if (foundItem == null)
			{
				foundItem = list.FirstOrDefault((SMItem item) => item.ItemType.ToLower() == ciIdentifier);
			}

			return foundItem;
		}

		/// <summary>
		/// Removes an item from a list recursivly looking through containers within containers.
		/// </summary>
		/// <returns><c>true</c>, if the item was removed, <c>false</c> otherwise.</returns>
		/// <param name="list">List to remove item from.</param>
		/// <param name="itemIdentifier">Item identifier.</param>
		private static bool RemoveItemFromListRecursive(List<SMItem> list, string itemIdentifier)
		{
			SMItem foundItem = FindItemInList(list, itemIdentifier);

			if (foundItem != null)
			{
				list.Remove(foundItem);
				return true;
			}

			foreach (SMItem item in list)
			{
				// TODO account for locked containers

				if (item.CanHoldOtherItems() && item.HeldItems != null)
				{
					foundItem = FindItemInListRecursive(item.HeldItems, itemIdentifier);
					if (foundItem != null)
					{
						item.HeldItems.Remove(foundItem);
						return true;
					}
				}
			}

			return false;
		}
	}
}
