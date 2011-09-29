﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace BattleNet
{
    class Container
    {
        protected String m_name;
        protected Int32 m_width, m_height;

        public List<Item> m_items;
        public List<BitArray> m_fields;


        public Container(String name, Int32 width, Int32 height)
        {
            m_items = new List<Item>();
            m_name = name;
            m_width = width;
            m_height = height;

            BitArray fieldLine = new BitArray(m_width, false);

            m_fields = new List<BitArray>(m_height);

            for (int i = 0; i < m_height; i++)
            {
                m_fields.Add(new BitArray(fieldLine));
            }


        }

        protected void SetItemFields(Item item, bool value)
        {
            try
            {
                for (int y = 0; y < item.height; y++)
                    for (int x = 0; x < item.width; x++)
                        m_fields[(int)item.y + y][(int)item.x + x] = value;
            }
            catch
            {
                Logging.Logger.Write("Coordinate Exception....");
            }
        }

        protected Boolean RectangleIsFree(int rectangleX, int rectangleY, int rectangleWidth, int rectangleHeight)
        {
            if ((rectangleX + rectangleWidth > m_width) || (rectangleY + rectangleHeight > m_height))
                return false;

            for (int y = rectangleY; y < rectangleY + rectangleHeight; y++)
            {
                for (int x = rectangleX; x < rectangleX + rectangleWidth; x++)
                {
                    if (m_fields[y][x])
                        return false;
                }
            }
            return true;
        }

        public void Add(Item item)
        {
            m_items.Add(item);
            SetItemFields(item, true);
        }

        public void Remove(Item item)
        {
            SetItemFields(item, false);
            m_items.Remove(item);
        }

        public UInt32 NumberFields()
        {
            UInt32 i = 0;
            for (Int32 y = 0; y < m_height; y++)
            {
                for (Int32 x = 0; x < m_width; x++)
                    if (m_fields[y][x])
                        i++;
            }

            return i;
        }

        public Boolean FindFreeSpace(Item item, out Coordinate output)
        {
            UInt16 item_width = item.width;
            UInt16 item_height = item.height;
            for (UInt16 y = 0; y < m_height; y++)
            {
                for (UInt16 x = 0; x < m_width; x++)
                {
                    if (RectangleIsFree(x, y, item.width, item.height))
                    {
                        output = new Coordinate(x, y);
                        return true;
                    }
                }
            }
            output = null;
            return false;
        }

        public Boolean FindFreeSpace(Item item)
        {
            Coordinate output;
            return FindFreeSpace(item, out output);
        }

    }
}