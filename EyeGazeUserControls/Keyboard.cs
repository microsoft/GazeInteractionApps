using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace EyeGazeUserControls
{
    public enum KeyType
    {
        Normal,
        Toggle,
        Layout
    }

    public class Keyboard : DependencyObject
    {
        public static readonly DependencyProperty KeyTypeProperty =
            DependencyProperty.RegisterAttached("KeyType", typeof(KeyType), typeof(Keyboard), new PropertyMetadata(0));

        public static KeyType GetKeyType(DependencyObject obj)
        {
            return (KeyType)obj.GetValue(KeyTypeProperty);
        }

        public static void SetKeyType(DependencyObject obj, KeyType value)
        {
            obj.SetValue(KeyTypeProperty, value);
        }

        public static readonly DependencyProperty VKProperty =
            DependencyProperty.RegisterAttached("VK", typeof(int), typeof(Keyboard), new PropertyMetadata(0));

        public static int GetVK(DependencyObject obj)
        {
            return (int)obj.GetValue(VKProperty);
        }

        public static void SetVK(DependencyObject obj, int value)
        {
            obj.SetValue(VKProperty, value);
        }

        public static readonly DependencyProperty VKListProperty =
            DependencyProperty.RegisterAttached("VKList", typeof(List<int>), typeof(Keyboard), new PropertyMetadata(0));

        public static List<int> GetVKList(DependencyObject obj)
        {
            var value = obj.GetValue(VKListProperty);
            var list = value as List<int>;
            if (list == null)
            {
                list = new List<int>();
                SetVKList(obj, list);
            }
            return list;
        }

        public static void SetVKList(DependencyObject obj, List<int> value)
        {
            obj.SetValue(VKListProperty, value);
        }

    }
}
