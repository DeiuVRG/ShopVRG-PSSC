#!/bin/bash
# View ShopVRG Azure SQL Database

clear
printf '\n%s\n%s\n%s\n\n' '========================================' '   ShopVRG Products - Azure SQL' '========================================' 

sqlcmd -S server-incercare.database.windows.net -d ShopVRG-db -U CloudSA7e00abbb -P V4l3NT1n2003 -C -h-1 -W -s'|' -Q 'SELECT Code as Cod, LEFT(Name,35) as Produs, Category as Cat, Price as Pret, Stock FROM Products ORDER BY Category'

printf '\n%s\n%s\n%s\n\n' '========================================' '   Sumar Categorii' '========================================'

sqlcmd -S server-incercare.database.windows.net -d ShopVRG-db -U CloudSA7e00abbb -P V4l3NT1n2003 -C -h-1 -W -s'|' -Q 'SELECT Category, COUNT(*) as Produse, SUM(Stock) as StocTotal FROM Products GROUP BY Category'

printf '\n'
