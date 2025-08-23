# GitHub Deploy Key (Write Access)
1) ssh-keygen -t ed25519 -C "pulsnet-deploy" -f ~/.ssh/pulsnet_netpulse -N ""
2) Add ~/.ssh/pulsnet_netpulse.pub at GitHub repo Settings > Deploy keys (Allow write).
3) eval "$(ssh-agent -s)" && ssh-add ~/.ssh/pulsnet_netpulse
4) git remote set-url origin git@github.com:ShaneSerrao/NetPulse.git && git fetch --all
5) ssh -T git@github.com (verify)
