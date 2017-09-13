
namespace PCEFTPOS.Client.API
{
    public class TransactionType
    {
        public static readonly TransactionType NotSet = new TransactionType(' ');
        public static readonly TransactionType PurchaseCash = new TransactionType('P');
        public static readonly TransactionType CashOut = new TransactionType('C');
        public static readonly TransactionType Refund = new TransactionType('R');
        public static readonly TransactionType PreAuth = new TransactionType('A');
        public static readonly TransactionType Completion = new TransactionType('M');
        public static readonly TransactionType TipAdjust = new TransactionType('T');
        public static readonly TransactionType Deposit = new TransactionType('D');
        public static readonly TransactionType Withdrawal = new TransactionType('W');
        public static readonly TransactionType Balance = new TransactionType('B');
        public static readonly TransactionType Voucher = new TransactionType('V');
        public static readonly TransactionType FundsTransfer = new TransactionType('F');
        public static readonly TransactionType OrderRequest = new TransactionType('O');
        public static readonly TransactionType MiniTransactionHistory = new TransactionType('H');
        public static readonly TransactionType AuthPIN = new TransactionType('X');
        public static readonly TransactionType EnhancedPIN = new TransactionType('K');

        public static readonly TransactionType Redemption = new TransactionType('K');
        public static readonly TransactionType RefundToCard = new TransactionType('K');
        public static readonly TransactionType CardSaleTopUp = new TransactionType('K');
        public static readonly TransactionType CardSale = new TransactionType('K');
        public static readonly TransactionType RefundFromCard = new TransactionType('K');
        public static readonly TransactionType CardBalance = new TransactionType('K');
        public static readonly TransactionType CardActivate = new TransactionType('K');
        public static readonly TransactionType AddPointsToCard = new TransactionType('K');
        public static readonly TransactionType DecrementPointsFromCard = new TransactionType('K');
        public static readonly TransactionType TransferPoints = new TransactionType('K');
        public static readonly TransactionType CashBackFromCard = new TransactionType('K');
        public static readonly TransactionType CancelVoid = new TransactionType('K');
        public static readonly TransactionType AddCard = new TransactionType('K');

        public static readonly TransactionType None = new TransactionType('0');

        public static char InternalCode { set; get; }

        public TransactionType(char code)
        {
            InternalCode = code;
        }
    }
}
