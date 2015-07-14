# FundBot
For help managing an investment portfolio across multiple trading accounts.


A buys.csv needs to be in the bin directory, along with a weightings.csv

buys.csv format:

XFN.TO,06/05/2014,100,29.97,CAD,CAD,Equity
AGG,06/05/2014,27,1000.69,USD,USD,Bonds
XIU.TO,05/05/2014,100,21.1,CAD,CAD,Equity
CPD.TO,05/05/2014,500,16.55,CAD,CAD,Preferred
EFA,05/05/2014,100,68.24,USD,INTL,Equity
...

weightings.csv format:

CAD,Equity,15
USD,Bonds,15
CAD,Bonds,15
CAD,Preferred,10
INTL,Equity,15
USD,Equity,25
CAD,REIT,5
