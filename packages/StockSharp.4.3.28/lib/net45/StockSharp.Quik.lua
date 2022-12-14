-------------------------------------------------------------------------
--  StockSharp QUIK Lua Fix Server (c) 2017 http://stocksharp.com      --
-------------------------------------------------------------------------

-- ??? ????????? ???????? ??????????????? ?????? ?????????.

-------------------------------------------------------------------------
--  ????????? ???????????
-------------------------------------------------------------------------
-- ??????? ???????????.
-- 1 - Verbose
-- 2 - Debug
-- 3 - Info
-- 4 - Warning
-- 5 - Error
-- 6 - Off
LogLevel=3

-- ???????? ?????????? ?????, ? ??????? ????? ??????????? ???-?????????.
-- LogFile="StockSharp.QuikLua.log"
-------------------------------------------------------------------------

-------------------------------------------------------------------------
--  ????????? QUIK Lua Fix ???????
-------------------------------------------------------------------------
-- ?????, ?? ??????? FIX ?????? ????? ???????????? ??????????.
-- TransactionAddress="127.0.0.1:5001"

-- ?????, ?? ??????? FIX ?????? ????? ????????? ??????-??????.
-- MarketDataAddress="127.0.0.1:5001"

-- ?????, ? ??????? ????????? ??????????? ? FIX ???????.
-- ???? ????? ?? ??????, ??????????? ??????????? ? ?????
-- ????????????? ? ??????? (??????? ?????????? ?????????????).
-- ServerLogin="quik"

-- ??????, ? ??????? ????????? ??????????? ? FIX ???????.
-- ServerPassword="quik"

-- https://forum.quik.ru/forum10/topic1218/
-- SingleSlash=false

-- ?????????????? ????????? ? ????????.
-- ConvertToLatin=true

-- ???????????? ????????? ?? ???????????.
-- IgnoreTransactionDuplicates=true
-------------------------------------------------------------------------

-------------------------------------------------------------------------
--  ????????? ??????-??????
-------------------------------------------------------------------------
-- ?????????? ????????? ?? ???????. ???? ?????????, ????????????
-- ?????? ???????.
-- ?? ?????????, ????????.
-- IncrementalDepthUpdates=false
-------------------------------------------------------------------------

-------------------------------------------------------------------------
--  ????? ????????? Lua (?? ????????)
-------------------------------------------------------------------------
package.path = ""
package.cpath = getScriptPath() .. "\\StockSharp.QuikLua.dll"

require("StockSharp")
-------------------------------------------------------------------------