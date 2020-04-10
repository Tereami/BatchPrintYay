﻿#region License
/*Данный код опубликован под лицензией Creative Commons Attribution-ShareAlike.
Разрешено использовать, распространять, изменять и брать данный код за основу для производных в коммерческих и
некоммерческих целях, при условии указания авторства и если производные лицензируются на тех же условиях.
Код поставляется "как есть". Автор не несет ответственности за возможные последствия использования.
Зуев Александр, 2020, все права защищены.
This code is listed under the Creative Commons Attribution-ShareAlike license.
You may use, redistribute, remix, tweak, and build upon this work non-commercially and commercially,
as long as you credit the author by linking back and license your new creations under the same terms.
This code is provided 'as is'. Author disclaims any implied warranty.
Zuev Aleksandr, 2020, all rigths reserved.*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace BatchPrintYay
{
    public enum ColorType { Monochrome, GrayScale, Color, MonochromeWithExcludes}

    public class PdfColor
    {
        private Color _color;
        private bool IsValid;
        public int ColorCode;


        public PdfColor()
        {
            IsValid = false;
        }

        public PdfColor(Color color)
        {
            _color = color;
            ColorCode = color.ToArgb();
            IsValid = true;
        }

        public Color GetColor()
        {
            if (!IsValid)
            {
                Color newColor = Color.FromArgb(ColorCode);
                _color = newColor;
            }
            IsValid = true;
            return _color;
        }
    }
}
