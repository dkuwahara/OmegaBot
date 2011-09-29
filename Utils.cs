/*  Copyright (c) 2010 Daniel Kuwahara
 *    This file is part of AlphaBot.

    AlphaBot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AlphaBot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with AlphaBot.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace BattleNet
{
    class Utils
    {
        public static UInt16 ReverseBytes(UInt16 value)
        {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        public static string readNullTerminatedString(string packet, ref int offset)
        {
            int zero = packet.IndexOf('\0',offset);
            string output;
            if (zero == -1)
            {
                zero = packet.Length;
                output = packet.Substring(offset, zero - offset);
                offset = 0;
            }
            else
            {
                output = packet.Substring(offset, zero - offset);
                offset = zero + 1;
            }
            return output;
        }

        public static UInt32 GetDifficulty(Client.GameDifficulty difficulty)
        {
	        switch(difficulty)
	        {
		        default:
		        case Client.GameDifficulty.NORMAL:
			        return 0x0000;
		        case Client.GameDifficulty.NIGHTMARE:
			        return 0x1000;
		        case Client.GameDifficulty.HELL:
			        return 0x2000;
	        }
        }

        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 97 )));
                if (i == 0)
                    builder.Append(Char.ToUpper(ch));
                else
                    builder.Append(ch);
            }

            return builder.ToString();
        }


    }
}
