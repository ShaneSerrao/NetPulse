### GitHub Deploy Key Setup (Write Access) for NetPulse

1) Generate deploy key
ssh-keygen -t ed25519 -C "pulsnet-deploy" -f ~/.ssh/pulsnet_netpulse -N ""
cat ~/.ssh/pulsnet_netpulse.pub

2) Add deploy key (write access)
- https://github.com/ShaneSerrao/NetPulse/settings/keys
- Add deploy key -> Title: pulsnet-deploy -> paste public key -> Allow write access

3) Configure SSH client
mkdir -p ~/.ssh && chmod 700 ~/.ssh
ssh-keyscan github.com >> ~/.ssh/known_hosts
eval "$(ssh-agent -s)"
ssh-add ~/.ssh/pulsnet_netpulse
cat >> ~/.ssh/config << 'EOC'
Host github.com-netpulse
  HostName github.com
  User git
  IdentityFile ~/.ssh/pulsnet_netpulse
  IdentitiesOnly yes
EOC
chmod 600 ~/.ssh/config

4) Point remote to SSH and test
git remote set-url origin git@github.com:ShaneSerrao/NetPulse.git
# or: git remote set-url origin github.com-netpulse:ShaneSerrao/NetPulse.git
git fetch --all
ssh -T git@github.com
# or: ssh -T github.com-netpulse
