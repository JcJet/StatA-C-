   M S B A M L   `   `   `  ????@  <StatA, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:StatA.WindowsStatA.Windows  5   	   *    $StatA.Windows.StrategySettingsDialog   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35a ]DevExpress.Xpf.Core.v17.1, Version=17.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49aH 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:StatA.Windows     5   	   X TXceed.Wpf.Toolkit, Version=2.6.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  5   	     ? Title$  StrategySettingsDialog??5	   	   $	??400??6(   $	??300??65   .??5
      ? $??	0,0,0,-21q?6    ? ColumnDefinitions 5   
   ?? 5      ?? 5       ? RowDefinitions 5   
   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   ?? 5      $
??Auto ?6   .+?5   
   ?? -   5         ??Name$ 
TextBlock0??5      $)??#Название стратегии??6(   $??WrapWithOverflow=?6B   ?? 5   
   -   5         ??Name$ NameText??5      $??1??6$    ?? TextWrapping$
 Wrap=?64   $??TextBox??6H   ?? 5   
   -   5        $ 
textBlock1??5      $i??cЧисло месяцев для вычисления исторического диапазона??6(   $??WrapWithOverflow=?6d   $??1??6?   %  Xceed.Wpf.Toolkit.DecimalUpDown  5   
   -   5          Name$ monthsInput??5      ?  ?Xceed.Wpf.Toolkit.NumericUpDown`1[[System.Nullable`1[[System.Decimal, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]   	Increment$ 1X?63   $??1??6A   $??1??6N   ?? 5    
   -   5        $ 
textBlock2??5       $s??mУровень стандартного отклонения спреда для входа в позицию??6(   $??WrapWithOverflow=?6j   $??2??6?     5!   
   -   5        $ sdInput??5!      $??2??6/   $??1??6<   ?? 5"   
   -   5        $ 
textBlock3??5"      $^??XМаксимальное P-значение теста на стационарность??6(   $??WrapWithOverflow=?6_   $??3??6     5#   
   -   5        $ 	pvalInput??5#      $??3??61   $??1??6>   ?? 5$   
   -	   5        $ 
textBlock4??5$      $??Число зон СКО??6(   $??WrapWithOverflow=?6=   $??4??6]     5%   
   -
   5        $ 
zonesInput??5%      $ 1X?62   $??4??6@   $??1??6M   ?? 5&   
   -   5        $ 
textBlock5??5&      $E???Способ расчета середины диапазона??6(   $??WrapWithOverflow=?6Q   $??5??6q   ?? 5'   
   -   5         ??Name$ rangeCalcInput??5'      $??5??6,   $??1??69   .@?5(      ?? -   5        $??History??5(      $??Исторический??6*   
	 ? Tag$	 History??6A   ?? 5)      -   5        $??CurrentSpread??5)      $??Текущий??60   $	 CurrentSpread??6B   5*      ?? 5+   
   -   5        $ 
textBlock6??5+      $)??#Отображение спреда??6(   $??WrapWithOverflow=?6B   $??6??6b   ?? 5,   
   -   5        $ spreadDrawInput??5,      $??6??6-   $??1??6:   .@?5-      ?? -   5        $??Spread??5-      $??Как есть??6)   $	 Spread??6<   ?? 5.      -   5        $??SpreadMA??5.      $#??Относительно МА??6+   $	 SpreadMA??6E   5/      ?? 50   
   -   5        $ 
textBlock7??50      $F??@Коэфф. изменения границы диапазона??6(   $??WrapWithOverflow=?6R   $??7??6r     51   
   -   5        $ channelInput??51      $ 1X?64   $??7??6B   $??1??6O   ?? 52   
   -   5        $ 
textBlock8??52      $/??)Расчет стационарности??6(   $??WrapWithOverflow=?6E   $??8??6e   ?? 53   
   -   5        $ stationarityCalcInput??53      $??8??63   $??1??6@   .@?54      ?? -   5        $??
StatSpread??54      $+??%Относительно центра??6-   $	 Spread??6K   ?? 55      -   5        $??StatSpreadMA??55      $#??Относительно МА??6/   $	 SpreadMA??6I   56      ?? 57   
   -   5        $ 
textBlock9??57      $<??6Источник исторических данных??6(   $??WrapWithOverflow=?6L   $??9??6l   ?? 58   
   -   5        $ historySourceInput??58      $??9??60   $??1??6=   .@?59      ?? -   5        $??Google??59      $??Google??6)   $	 Google??6:   ?? 5:      -   5        $??Quindl??5:      $??Quindl??6)   $	 Quindl??6:   ?? 5;      -   5        $??Xignite??5;      $??Xignite??6*   $	 Xignite??6<   ?? 5<      -   5        $??Yahoo??5<      $??Yahoo??6(   $	 Yahoo??68   ?? 5=      -   5        $
??Alor??5=      $
??Alor??6'   $
	 Alor??66   ?? 5>      -    5        $??Finam??5>      $??
Финам??6(   $	 Finam??68   ?? 5?      -!   5        $	??Mfd??5?      $	??Mfd??6&   $		 Mfd??64   ?? 5@      -"   5        $??Ux??5@      $??Ux??6%   $	 Ux??62   5A      ?? 5C   
   -#   5        
 ??Name$
 OkButton??5C      $??OK??6#   $??10??60    ?? 	IsDefault . 6U   ?? 5D   
   -$   5        $
 CancelButton??5D      $??Отмена??6'   $??10??68   $??1??6F    ?? IsCancel . 6f   5E      5F      